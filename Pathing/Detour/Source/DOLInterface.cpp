#include <cstdio>
#include <cstring>
#include <exception>
#include <functional>
#include <iostream>
#include <random>

#include "dol_detour.hpp"

/*
	[DllImport("dol_detour", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	private static extern bool LoadNavMesh(string file, ref IntPtr meshPtr, ref IntPtr queryPtr);

	[DllImport("dol_detour", CallingConvention = CallingConvention.Cdecl)]
	private static extern bool FreeNavMesh(IntPtr meshPtr, IntPtr queryPtr);

	[DllImport("dol_detour", CallingConvention = CallingConvention.Cdecl)]
	private static extern dtStatus PathStraight(IntPtr queryPtr, float[] start, float[] end, float[] polyPickExt, dtPolyFlags[] queryFilter, dtStraightPathOptions pathOptions, ref int pointCount, float[] pointBuffer, dtPolyFlags[] pointFlags);

	[DllImport("dol_detour", CallingConvention = CallingConvention.Cdecl)]
	private static extern dtStatus FindRandomPointAroundCircle(IntPtr queryPtr, float[] center, float radius, float[] polyPickExt, dtPolyFlags[] queryFilter, float[] outputVector);

	[DllImport("dol_detour", CallingConvention = CallingConvention.Cdecl)]
	private static extern dtStatus FindClosestPoint(IntPtr queryPtr, float[] center, float[] polyPickExt, dtPolyFlags[] queryFilter, float[] outputVector);

	[DllImport("dol_detour", CallingConvention = CallingConvention.Cdecl)]
	private static extern dtStatus GetPolyAt(IntPtr queryPtr, float[] center, float[] polyPickExt, dtPolyFlags[] queryFilter, ref uint outputPolyRef, float[] outputVector);

	[DllImport("dol_detour", CallingConvention = CallingConvention.Cdecl)]
	private static extern dtStatus SetPolyFlags(IntPtr meshPtr, uint polyRef, dtPolyFlags flags);

	[DllImport("dol_detour", CallingConvention = CallingConvention.Cdecl)]
	private static extern dtStatus QueryPolygons(IntPtr queryPtr, float[] center, float[] polyPickExt, dtPolyFlags[] queryFilter, uint[] outputPolyRefs, ref int outputPolyCount, int maxPolyCount);
*/

// RAII helper
struct RAII
{
	std::function<void()> cleaner;
	RAII(std::function<void()> cleaner) : cleaner(cleaner) {}
	~RAII() { this->cleaner(); }
};

// missing from Detour?
struct dtNavMeshSetHeader
{
	std::int32_t magic;
	std::int32_t version;
	std::int32_t numTiles;
	dtNavMeshParams params;
};
struct dtNavMeshTileHeader
{
	dtTileRef ref;
	std::int32_t size;
};

DLLEXPORT bool LoadNavMesh(char const *file, dtNavMesh **const mesh)
{
	// load the file
	auto fp = std::fopen(file, "rb");
	if (!fp)
		return false;

	// scope for fp closing
	{
		auto _fpRAII = RAII([=]
							{ std::fclose(fp); });

		dtNavMeshSetHeader header;
		fread(&header, sizeof(header), 1, fp);

		if (header.magic != 0x4d534554 || header.version != 1)
			return false;

		// init mesh and query
		*mesh = dtAllocNavMesh();
		auto status = (*mesh)->init(&header.params);
		if (dtStatusFailed(status))
		{
			dtFreeNavMesh(*mesh);
			*mesh = nullptr;
			return false;
		}
		if (header.numTiles > 0)
		{
			auto tileIdx = 0;
			while (tileIdx < header.numTiles)
			{
				dtNavMeshTileHeader tileHeader;
				fread(&tileHeader, sizeof(tileHeader), 1, fp);
				void *data;
				if (tileHeader.ref == 0 || tileHeader.size == 0 || (data = dtAlloc(tileHeader.size, DT_ALLOC_PERM)) == 0)
					break;
				memset(data, 0, tileHeader.size);
				fread(data, tileHeader.size, 1, fp);
				(*mesh)->addTile((unsigned char *)data, tileHeader.size, 1, tileHeader.ref, nullptr);
				tileIdx += 1;
			}
		}
	}
	return true;
}

DLLEXPORT bool FreeNavMesh(dtNavMesh *meshPtr)
{
	if (meshPtr)
		dtFreeNavMesh(meshPtr);
	return true;
}

DLLEXPORT bool CreateNavMeshQuery(dtNavMesh *mesh, dtNavMeshQuery **const query)
{

	*query = dtAllocNavMeshQuery();
	auto status = (*query)->init(mesh, 2048);
	if (dtStatusFailed(status))
	{
		dtFreeNavMeshQuery(*query);
		*query = nullptr;
		return false;
	}
	return true;
}
DLLEXPORT bool FreeNavMeshQuery(dtNavMeshQuery *queryPtr)
{
	if (queryPtr)
		dtFreeNavMeshQuery(queryPtr);
	return true;
}

static inline bool IsMidPointOnPath(float const* A, float const* B, float const* C)
{
	float vectAC[3];
	dtVsub(vectAC, C, A);
	dtVnormalize(vectAC);
	float vectAB[3];
	dtVsub(vectAB, B, A);
	float cross[3];
	dtVcross(cross, vectAB, vectAC);
	float len = dtVlen(cross);
	return len <= 1;
}

void PathOptimize(dtNavMeshQuery *query, int *pointCount, float *pointBuffer, dtPolyRef *refs)
{
	for (int i = 0; i < *pointCount - 2; ++i)
	{
		unsigned short flags[2];
		query->getAttachedNavMesh()->getPolyFlags(refs[i + 0], flags + 0);
		query->getAttachedNavMesh()->getPolyFlags(refs[i + 1], flags + 1);
		if (flags[0] != flags[1]) // we can't merge 2 different points
			continue;

		// we take 3 points: first --- mid --- last and check if mid is on the line, in this case, we remove mid
		float const *A = &(pointBuffer[(i + 0) * 3]);
		float const *B = &(pointBuffer[(i + 1) * 3]); // mid, point to remove
		float const *C = &(pointBuffer[(i + 2) * 3]);

		if (IsMidPointOnPath(A, B, C))
		{
			std::copy(pointBuffer + (i + 2) * 3, pointBuffer + (*pointCount) * 3, pointBuffer + (i + 1) * 3);
			std::copy(refs + i + 2, refs + *pointCount, refs + i + 1);
			*pointCount -= 1;
			--i; // we redo this loop
		}
	}
}

DLLEXPORT dtStatus PathStraight(dtNavMeshQuery *query, float start[], float end[], float polyPickExt[], dtPolyFlags queryFilter[], dtStraightPathOptions pathOptions, int *pointCount, float *pointBuffer, dtPolyFlags *pointFlags)
{
	dtStatus status;
	*pointCount = 0;

	dtPolyRef startRef;
	dtPolyRef endRef;
	dtQueryFilter filter;
	filter.setIncludeFlags(queryFilter[0]);
	filter.setExcludeFlags(queryFilter[1]);
	if (dtStatusSucceed(status = query->findNearestPoly(start, polyPickExt, &filter, &startRef, nullptr)) && dtStatusSucceed(status = query->findNearestPoly(end, polyPickExt, &filter, &endRef, nullptr)))
	{
		int npolys = 0;
		dtPolyRef polys[MAX_POLY];
		if (dtStatusSucceed(status = query->findPath(startRef, endRef, start, end, &filter, polys, &npolys, MAX_POLY)))
		{
			float epos[3];
			epos[0] = end[0];
			epos[1] = end[1];
			epos[2] = end[2];
			if ((polys[npolys + -1] == endRef) || dtStatusSucceed(status = query->closestPointOnPoly(polys[npolys + -1], end, epos, nullptr)))
			{
				dtPolyRef straightPathPolys[MAX_POLY];
				unsigned char straightPathFlags[MAX_POLY];
				auto straightPathRefs = &straightPathPolys[0];
				if (dtStatusSucceed(status = query->findStraightPath(start, epos, polys, npolys, pointBuffer, straightPathFlags, straightPathRefs, pointCount, MAX_POLY, pathOptions)) && (0 < *pointCount))
				{
					PathOptimize(query, pointCount, pointBuffer, straightPathRefs);
					int pointIdx = 0;
					while (*pointCount != pointIdx && pointIdx <= *pointCount)
					{
						auto ref = *straightPathRefs;
						pointIdx = pointIdx + 1;
						straightPathRefs = straightPathRefs + 1;
						query->getAttachedNavMesh()->getPolyFlags(ref, (unsigned short *)pointFlags);
						pointFlags = pointFlags + 1;
					}
				}
			}
		}
	}
	return status;
}

thread_local std::mt19937 rngMt = std::mt19937(std::random_device{}());
thread_local std::uniform_real_distribution<float> rng(0.0f, 1.0f);

float frand()
{
	return rng(rngMt);
}

DLLEXPORT dtStatus FindRandomPointAroundCircle(dtNavMeshQuery *query, float center[], float radius, float polyPickExt[], dtPolyFlags queryFilter[], float *outputVector)
{
	dtQueryFilter filter;
	filter.setIncludeFlags(queryFilter[0]);
	filter.setExcludeFlags(queryFilter[1]);
	dtPolyRef centerRef;
	auto status = query->findNearestPoly(center, polyPickExt, &filter, &centerRef, nullptr);
	if (dtStatusSucceed(status))
	{
		dtPolyRef outRef;
		status = query->findRandomPointAroundCircle(centerRef, center, radius, &filter, frand, &outRef, outputVector);
	}
	return status;
}

DLLEXPORT dtStatus FindClosestPoint(dtNavMeshQuery *query, float center[], float polyPickExt[], dtPolyFlags queryFilter[], float *outputVector)
{
	dtQueryFilter filter;
	filter.setIncludeFlags(queryFilter[0]);
	filter.setExcludeFlags(queryFilter[1]);
	dtPolyRef centerRef;
	auto status = query->findNearestPoly(center, polyPickExt, &filter, &centerRef, nullptr);
	if (dtStatusSucceed(status))
		status = query->closestPointOnPoly(centerRef, center, outputVector, nullptr);
	return status;
}

DLLEXPORT dtStatus GetPolyAt(dtNavMeshQuery *query, float *center, float *extents, unsigned short *queryFilter, dtPolyRef *polyRef, float *point)
{
	dtQueryFilter filter;
	filter.setIncludeFlags(queryFilter[0]);
	filter.setExcludeFlags(queryFilter[1]);
	return query->findNearestPoly(center, extents, &filter, polyRef, point);
}

DLLEXPORT dtStatus SetPolyFlags(dtNavMesh *navMesh, dtPolyRef ref, unsigned short flags)
{
	return navMesh->setPolyFlags(ref, flags);
}

DLLEXPORT dtStatus QueryPolygons(dtNavMeshQuery *query, float *center, float *polyPickExtents, unsigned short *queryFilter, dtPolyRef *polys, int *polyCount, int maxPolys)
{
	dtQueryFilter filter;
	filter.setIncludeFlags(queryFilter[0]);
	filter.setExcludeFlags(queryFilter[1]);
	return query->queryPolygons(center, polyPickExtents, &filter, polys, polyCount, maxPolys);
}
