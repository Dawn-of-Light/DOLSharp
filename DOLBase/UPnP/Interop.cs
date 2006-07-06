/*
 * DAWN OF LIGHT - The first free open source DAoC server emulator
 * 
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation; either version 2
 * of the License, or (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.
 *
 */


using System.Collections;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.CustomMarshalers;

namespace DOL.NatTraversal.Interop
{

	[ComImport, Guid("624BD588-9060-4109-B0B0-1ADBBCAC32DF"), TypeLibType(4160)]
	internal interface INATEventManager
	{
		[DispId(1)]
		object ExternalIPAddressCallback
		{
			[param: In, MarshalAs(UnmanagedType.IUnknown)]
			[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(1)]
			set;
		}
		[DispId(2)]
		object NumberOfEntriesCallback
		{
			[param: In, MarshalAs(UnmanagedType.IUnknown)]
			[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(2)]
			set;
		}
	}

	[ComImport, TypeLibType(4160), Guid("6F10711F-729B-41E5-93B8-F21D0F818DF1")]
	internal interface IStaticPortMapping
	{
		[DispId(1)]
		string ExternalIPAddress
		{
			[return: MarshalAs(UnmanagedType.BStr)]
			[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(1)]
			get;
		}
		[DispId(2)]
		int ExternalPort
		{
			[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(2)]
			get;
		}
		[DispId(3)]
		int InternalPort
		{
			[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(3)]
			get;
		}
		[DispId(4)]
		string Protocol
		{
			[return: MarshalAs(UnmanagedType.BStr)]
			[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(4)]
			get;
		}
		[DispId(5)]
		string InternalClient
		{
			[return: MarshalAs(UnmanagedType.BStr)]
			[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(5)]
			get;
		}
		[DispId(6)]
		bool Enabled
		{
			[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(6)]
			get;
		}
		[DispId(7)]
		string Description
		{
			[return: MarshalAs(UnmanagedType.BStr)]
			[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(7)]
			get;
		}
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(8)]
		void EditInternalClient([In, MarshalAs(UnmanagedType.BStr)] string bstrInternalClient);
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(9)]
		void Enable([In] bool vb);
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(10)]
		void EditDescription([In, MarshalAs(UnmanagedType.BStr)] string bstrDescription);
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(11)]
		void EditInternalPort([In] int lInternalPort);
	}

	[ComImport, Guid("CD1F3E77-66D6-4664-82C7-36DBB641D0F1"), TypeLibType(4160)]
	internal interface IStaticPortMappingCollection /*: IEnumerable*/ {
		[return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(EnumeratorToEnumVariantMarshaler))]
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(-4), TypeLibFunc(0x41)]
		IEnumerator GetEnumerator();

		[DispId(0)]
		IStaticPortMapping this[int lExternalPort, string bstrProtocol]
		{
			[return: MarshalAs(UnmanagedType.Interface)]
			[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0)]
			get;
		}
		[DispId(1)]
		int Count
		{
			[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(1)]
			get;
		}
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(2)]
		void Remove([In] int lExternalPort, [In, MarshalAs(UnmanagedType.BStr)] string bstrProtocol);
		[return: MarshalAs(UnmanagedType.Interface)]
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(3)]
		IStaticPortMapping Add([In] int lExternalPort, [In, MarshalAs(UnmanagedType.BStr)] string bstrProtocol, [In] int lInternalPort, [In, MarshalAs(UnmanagedType.BStr)] string bstrInternalClient, [In] bool bEnabled, [In, MarshalAs(UnmanagedType.BStr)] string bstrDescription);
	}

	[ComImport, Guid("B171C812-CC76-485A-94D8-B6B3A2794E99"), TypeLibType(4160)]
	internal interface IUPnPNAT
	{
		[DispId(1)]
		IStaticPortMappingCollection StaticPortMappingCollection
		{
			[return: MarshalAs(UnmanagedType.Interface)]
			[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(1)]
			get;
		}
		[DispId(2)]
		object /*IDynamicPortMappingCollection*/ DynamicPortMappingCollection
		{
			[return: MarshalAs(UnmanagedType.Interface)]
			[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(2)]
			get;
		}
		[DispId(3)]
		INATEventManager NATEventManager
		{
			[return: MarshalAs(UnmanagedType.Interface)]
			[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(3)]
			get;
		}
	}

	[ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("9C416740-A34E-446F-BA06-ABD04C3149AE")]
	internal interface INATExternalIPAddressCallback
	{
		void NewExternalIPAddress([MarshalAs(UnmanagedType.BStr)]string newExternalIPAddress);
	}

	[ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("C83A0A74-91EE-41B6-B67A-67E0F00BBD78")]
	internal interface INATNumberOfEntriesCallback
	{
		void NewNumberOfEntries(int newNumberOfEntries);
	}

	[ComImport, Guid("B171C812-CC76-485A-94D8-B6B3A2794E99"), CoClass(typeof(UPnPNATCreator))]
	internal interface UPnPNAT : IUPnPNAT
	{
	}

	[ComImport, TypeLibType(2), Guid("AE1E00AA-3FD5-403C-8A27-2BBDC30CD0E1"), ClassInterface(ClassInterfaceType.None)]
	internal class UPnPNATCreator
	{
	}
}


