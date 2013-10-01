using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Xml;
using System.IO;

public class UPnPNat
{
	public class PortForwading
	{
		public IPAddress internalIP;
		public int internalPort;
		public int externalPort;
		public ProtocolType protocol;
		public string description;
		public bool enabled;

		public PortForwading()
		{
			internalIP = IPAddress.None;
			internalPort = 0;
			externalPort = 0;
			protocol = ProtocolType.Unspecified;
			description = "(Unknown)";
			enabled = false;
		}
	}

	private string _serviceUrl;

	private static string _CombineUrls(string resp, string p)
	{
		int n = resp.IndexOf("://");
		n = resp.IndexOf('/', n + 3);
		return resp.Substring(0, n) + p;
	}

	private static string _GetServiceUrl(string resp)
	{
		XmlDocument desc = new XmlDocument();
		desc.Load(WebRequest.Create(resp).GetResponse().GetResponseStream());
		XmlNamespaceManager nsMgr = new XmlNamespaceManager(desc.NameTable);
		nsMgr.AddNamespace("tns", "urn:schemas-upnp-org:device-1-0");
		XmlNode typen = desc.SelectSingleNode("//tns:device/tns:deviceType/text()", nsMgr);
		if (!typen.Value.Contains("InternetGatewayDevice"))
			return null;
		XmlNode node = desc.SelectSingleNode(
			"//tns:service[tns:serviceType=\"urn:schemas-upnp-org:service:WANIPConnection:1\"]/tns:controlURL/text()",
			nsMgr);
		if (node == null)
			return null;
		XmlNode eventnode = desc.SelectSingleNode(
			"//tns:service[tns:serviceType=\"urn:schemas-upnp-org:service:WANIPConnection:1\"]/tns:eventSubURL/text()",
			nsMgr);
		_CombineUrls(resp, eventnode.Value);
		return _CombineUrls(resp, node.Value);
	}

	private static XmlDocument _GetSOAPRequest(string url, string soap, string function)
	{
		string req = "<?xml version=\"1.0\"?>" +
			"<s:Envelope xmlns:s=\"http://schemas.xmlsoap.org/soap/envelope/\" s:encodingStyle=\"http://schemas.xmlsoap.org/soap/encoding/\">" +
			"<s:Body>" +
			soap +
			"</s:Body>" +
			"</s:Envelope>";
		WebRequest r = WebRequest.Create(url);
		r.Method = "POST";
		byte[] b = Encoding.UTF8.GetBytes(req);
		r.Headers.Add("SOAPACTION", "\"urn:schemas-upnp-org:service:WANIPConnection:1#" + function + "\"");
		r.ContentType = "text/xml; charset=\"utf-8\"";
		r.ContentLength = b.Length;
		r.GetRequestStream().Write(b, 0, b.Length);
		XmlDocument resp = new XmlDocument();
		WebResponse wres = r.GetResponse();
		Stream ress = wres.GetResponseStream();
		resp.Load(ress);
		return resp;
	}

	public UPnPNat()
	{
	}

	/// <summary>
	/// Discover network
	/// </summary>
	/// <returns>True if an UPnP Gateway is available</returns>
	public bool Discover()
	{
		Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
		s.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);
		const string req = "M-SEARCH * HTTP/1.1\r\n" +
			"HOST: 239.255.255.250:1900\r\n" +
			"ST:upnp:rootdevice\r\n" +
			"MAN:\"ssdp:discover\"\r\n" +
			"MX:3\r\n\r\n";
		byte[] data = Encoding.ASCII.GetBytes(req);
		IPEndPoint ipe = new IPEndPoint(IPAddress.Broadcast, 1900);
		byte[] buffer = new byte[0x1000];

		// Graveen: quickfix
		s.ReceiveTimeout = 1000;

		s.SendTo(data, ipe);
		s.SendTo(data, ipe);
		s.SendTo(data, ipe);

		int length;
		do
		{
			length = s.Receive(buffer);

			string resp = Encoding.ASCII.GetString(buffer, 0, length).ToLower();
			if (resp.Contains("upnp:rootdevice"))
			{
				resp = resp.Substring(resp.ToLower().IndexOf("location:") + 9);
				resp = resp.Substring(0, resp.IndexOf("\r")).Trim();
				if (!string.IsNullOrEmpty(_serviceUrl = _GetServiceUrl(resp)))
				{
					return true;
				}
			}
		} while (length > 0);

		return false;
	}

	/// <summary>
	/// Get list of forwarded port on UPnP gateway
	/// </summary>
	/// <returns></returns>
	public List<PortForwading> ListForwardedPort()
	{
		if (string.IsNullOrEmpty(_serviceUrl))
			throw new Exception("No UPnP service available or Discover() has not been called");

		List<PortForwading> forwadedPort = new List<PortForwading>();
		for (int index = 0; ; ++index)
		{
			try
			{
				XmlDocument xdoc = _GetSOAPRequest(
					_serviceUrl,
					"<m:GetGenericPortMappingEntry xmlns:m=\"urn:schemas-upnp-org:service:WANIPConnection:1\">" +
					"<NewPortMappingIndex>" + index + "</NewPortMappingIndex>" +
					"</m:GetGenericPortMappingEntry>",
					"GetGenericPortMappingEntry");
				XmlNamespaceManager nsMgr = new XmlNamespaceManager(xdoc.NameTable);
				nsMgr.AddNamespace("tns", "urn:schemas-upnp-org:device-1-0");
				PortForwading port = new PortForwading();
				XmlNode node = xdoc.SelectSingleNode("//NewInternalClient/text()", nsMgr);
				if (node != null)
					port.internalIP = IPAddress.Parse(node.Value);
				node = xdoc.SelectSingleNode("//NewInternalPort/text()", nsMgr);
				if (node != null)
					port.internalPort = int.Parse(node.Value);
				node = xdoc.SelectSingleNode("//NewExternalPort/text()", nsMgr);
				if (node != null)
					port.externalPort = int.Parse(node.Value);
				node = xdoc.SelectSingleNode("//NewProtocol/text()", nsMgr);
				if (node != null)
					port.protocol = (ProtocolType)Enum.Parse(typeof(ProtocolType), node.Value, true);
				node = xdoc.SelectSingleNode("//NewPortMappingDescription/text()", nsMgr);
				if (node != null)
					port.description = node.Value;
				node = xdoc.SelectSingleNode("//NewEnabled/text()", nsMgr);
				if (node != null)
					port.enabled = node.Value != "0";
				forwadedPort.Add(port);
			}
			catch (WebException)
			{
				break;
			}
		}
		return forwadedPort;
	}

	/// <summary>
	/// Forward a port
	/// </summary>
	/// <param name="internalPort">Port number on LAN interface</param>
	/// <param name="externalPort">Port number on WAN interface</param>
	/// <param name="protocol">Protocol (TCP or UDP)</param>
	/// <param name="description">Describe service behind this forwading rule</param>
	public void ForwardPort(int internalPort, int externalPort, ProtocolType protocol, string description)
	{
		ForwardPort(internalPort, externalPort, protocol, description, IPAddress.Any);
	}

	/// <summary>
	/// Forward a port
	/// </summary>
	/// <param name="internalPort">Port number on LAN interface</param>
	/// <param name="externalPort">Port number on WAN interface</param>
	/// <param name="protocol">Protocol (TCP or UDP)</param>
	/// <param name="description">Describe service behind this forwading rule</param>
	/// <param name="internalIp">LAN ip</param>
	public void ForwardPort(int internalPort, int externalPort, ProtocolType protocol, string description, IPAddress internalIp)
	{
		if (string.IsNullOrEmpty(_serviceUrl))
			throw new Exception("No UPnP service available or Discover() has not been called");

		if (internalIp == null || internalIp.ToString() == IPAddress.Any.ToString())
		{
			foreach (var ip in Dns.GetHostAddresses(Dns.GetHostName()))
				if (ip.GetAddressBytes().Length == 4)
			{
				internalIp = ip;
				break;
			}
			if (internalIp == null)
				throw new Exception("LAN IP not found !");
		}
		_GetSOAPRequest(
			_serviceUrl,
			"<u:AddPortMapping xmlns:u=\"urn:schemas-upnp-org:service:WANIPConnection:1\">" +
			"<NewRemoteHost></NewRemoteHost>" +
			"<NewExternalPort>" + externalPort + "</NewExternalPort>" +
			"<NewProtocol>" + protocol.ToString().ToUpper() + "</NewProtocol>" +
			"<NewInternalPort>" + internalPort + "</NewInternalPort>" +
			"<NewInternalClient>" + internalIp + "</NewInternalClient>" +
			"<NewEnabled>1</NewEnabled>" +
			"<NewPortMappingDescription>" + description + "</NewPortMappingDescription>" +
			"<NewLeaseDuration>0</NewLeaseDuration>" +
			"</u:AddPortMapping>",
			"AddPortMapping");
	}

	/// <summary>
	/// Remove a forwading rule
	/// </summary>
	/// <param name="port">Port number on WAN interface</param>
	/// <param name="protocol">Protocol (TCP or UDP)</param>
	public void DeleteForwardingRule(int port, ProtocolType protocol)
	{
		if (string.IsNullOrEmpty(_serviceUrl))
			throw new Exception("No UPnP service available or Discover() has not been called");

		_GetSOAPRequest(
			_serviceUrl,
			"<u:DeletePortMapping xmlns:u=\"urn:schemas-upnp-org:service:WANIPConnection:1\">" +
			"<NewRemoteHost></NewRemoteHost>" +
			"<NewExternalPort>" + port + "</NewExternalPort>" +
			"<NewProtocol>" + protocol.ToString().ToUpper() + "</NewProtocol>" +
			"</u:DeletePortMapping>",
			"DeletePortMapping");
	}

	/// <summary>
	/// Get WAN IP
	/// </summary>
	/// <returns></returns>
	public IPAddress GetExternalIP()
	{
		if (string.IsNullOrEmpty(_serviceUrl))
			throw new Exception("No UPnP service available or Discover() has not been called");
		XmlDocument xdoc = _GetSOAPRequest(
			_serviceUrl,
			"<u:GetExternalIPAddress xmlns:u=\"urn:schemas-upnp-org:service:WANIPConnection:1\"></u:GetExternalIPAddress>",
			"GetExternalIPAddress");
		XmlNamespaceManager nsMgr = new XmlNamespaceManager(xdoc.NameTable);
		nsMgr.AddNamespace("tns", "urn:schemas-upnp-org:device-1-0");
		string IP = xdoc.SelectSingleNode("//NewExternalIPAddress/text()", nsMgr).Value;
		return IPAddress.Parse(IP);
	}
}
