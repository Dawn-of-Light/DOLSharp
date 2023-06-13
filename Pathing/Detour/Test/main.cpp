#include "dol_detour.hpp"

#include <chrono>
#include <filesystem>
#include <iostream>
#include <thread>
#include <vector>

auto const FACTOR = 1.0 / 32.0f;

static dtNavMesh *navMesh;
static dtNavMeshQuery *query;

static auto defaultInclude = (dtPolyFlags)(dtPolyFlags::ALL ^ dtPolyFlags::DISABLED);
static auto defaultExclude = (dtPolyFlags)0;
static dtPolyFlags filter[] = {defaultInclude, defaultExclude};

void test_FindRandomPointAroundCircle(dtNavMeshQuery *query)
{
    for (int i = 0; i < 1000; ++i)
    {
        float center[] = {31000 * FACTOR, 15800 * FACTOR, 33750 * FACTOR};
        float polyPick[] = {2.0f, 4.0f, 2.0f};
        float output[3];
        auto status = FindRandomPointAroundCircle(query, center, 512 * FACTOR, polyPick, filter, output);
        if (!dtStatusSucceed(status))
            throw i;
    }
}

void test_FindClosestPoint(dtNavMeshQuery *query)
{
    for (int i = 0; i < 1000; ++i)
    {
        float center[] = {31000 * FACTOR, 15800 * FACTOR, 33750 * FACTOR};
        float range[] = {512 * FACTOR, 256 * FACTOR, 512 * FACTOR};
        float output[3];
        auto status = FindClosestPoint(query, center, range, filter, output);
        if (!dtStatusSucceed(status))
            throw i;
    }
}

void test_PathStraight__AREA(dtNavMeshQuery *query)
{
    for (int i = 0; i < 1000; ++i)
    {
        float start[] = {30893 * FACTOR, 15637 * FACTOR, 33758 * FACTOR};
        float end[] = {31095 * FACTOR, 15511 * FACTOR, 33902 * FACTOR};
        float polyPick[] = {64 * FACTOR, 256 * FACTOR, 64 * FACTOR};
        int pointCount;
        float pointBuffer[MAX_POLY];
        dtPolyFlags pointFlags[MAX_POLY];
        auto status = PathStraight(query, start, end, polyPick, filter, dtStraightPathOptions::DT_STRAIGHTPATH_AREA_CROSSINGS, &pointCount, pointBuffer, pointFlags);
        if (!dtStatusSucceed(status))
            throw i;
    }
}

void test_PathStraight__ALL(dtNavMeshQuery *query)
{
    for (int i = 0; i < 1000; ++i)
    {
        float start[] = {30893 * FACTOR, 15637 * FACTOR, 33758 * FACTOR};
        float end[] = {31095 * FACTOR, 15511 * FACTOR, 33902 * FACTOR};
        float polyPick[] = {64 * FACTOR, 256 * FACTOR, 64 * FACTOR};
        int pointCount;
        float pointBuffer[MAX_POLY];
        dtPolyFlags pointFlags[MAX_POLY];
        auto status = PathStraight(query, start, end, polyPick, filter, dtStraightPathOptions::DT_STRAIGHTPATH_ALL_CROSSINGS, &pointCount, pointBuffer, pointFlags);
        if (!dtStatusSucceed(status))
            throw i;
    }
}

int main(int ac, char const *const *av)
{
    if (!std::filesystem::exists("./zone078.nav"))
        std::filesystem::current_path("..");
    if (!std::filesystem::exists("./zone078.nav"))
    {
        std::cerr << "zone078.nav not found" << std::endl;
        return 1;
    }

    std::cout << "Load nav mesh zone078.nav: ";
    if (!LoadNavMesh("zone078.nav", &navMesh))
    {
        std::cout << "KO" << std::endl;
        return 1;
    }
    std::cout << "OK" << std::endl;
    std::cout << "Create nav mesh query";
    if (!CreateNavMeshQuery(navMesh, &query))
    {
        std::cout << "KO" << std::endl;
        return 1;
    }
    std::cout << "OK" << std::endl;

#define TEST(func)                                                                                                                                                \
    do                                                                                                                                                            \
    {                                                                                                                                                             \
        std::cout << #func << "...";                                                                                                                              \
        auto start = std::chrono::system_clock::now();                                                                                                            \
        try                                                                                                                                                       \
        {                                                                                                                                                         \
            func(query);                                                                                                                                          \
            std::cout << "OK (" << std::chrono::duration_cast<std::chrono::milliseconds>(std::chrono::system_clock::now() - start).count() << "ms)" << std::endl; \
        }                                                                                                                                                         \
        catch (...)                                                                                                                                               \
        {                                                                                                                                                         \
            std::cout << "KO" << std::endl;                                                                                                                       \
        }                                                                                                                                                         \
    } while (0)

    TEST(test_FindRandomPointAroundCircle);
    TEST(test_FindClosestPoint);
    TEST(test_PathStraight__AREA);
    TEST(test_PathStraight__ALL);

    std::cout << "=== MULTIHREADS ===\n";

#define TEST_THREADED(func)                                                                                                                                       \
    do                                                                                                                                                            \
    {                                                                                                                                                             \
        std::cout << #func << "...";                                                                                                                              \
        auto start = std::chrono::system_clock::now();                                                                                                            \
        try                                                                                                                                                       \
        {                                                                                                                                                         \
            std::vector<std::thread> threads;                                                                                                                     \
            std::vector<dtNavMeshQuery *> queries;                                                                                                                \
            for (int i = 0; i < 16; ++i)                                                                                                                          \
            {                                                                                                                                                     \
                dtNavMeshQuery *query;                                                                                                                            \
                CreateNavMeshQuery(navMesh, &query);                                                                                                              \
                queries.push_back(query);                                                                                                                         \
                threads.emplace_back(func, query);                                                                                                                \
            }                                                                                                                                                     \
            for (auto &t : threads)                                                                                                                               \
                t.join();                                                                                                                                         \
            std::cout << "OK (" << std::chrono::duration_cast<std::chrono::milliseconds>(std::chrono::system_clock::now() - start).count() << "ms)" << std::endl; \
            for (auto query : queries)                                                                                                                            \
                FreeNavMeshQuery(query);                                                                                                                          \
        }                                                                                                                                                         \
        catch (...)                                                                                                                                               \
        {                                                                                                                                                         \
            std::cout << "KO" << std::endl;                                                                                                                       \
        }                                                                                                                                                         \
    } while (0)

    TEST_THREADED(test_FindRandomPointAroundCircle);
    TEST_THREADED(test_FindClosestPoint);
    TEST_THREADED(test_PathStraight__AREA);
    TEST_THREADED(test_PathStraight__ALL);

    std::cout << "Free nav mesh query: ";
    if (!FreeNavMeshQuery(query))
    {
        std::cout << "KO" << std::endl;
        return 1;
    }
    std::cout << "OK" << std::endl;
    std::cout << "Free nav mesh: ";
    if (!FreeNavMesh(navMesh))
    {
        std::cout << "KO" << std::endl;
        return 1;
    }
    std::cout << "OK" << std::endl;
    return 0;
}
