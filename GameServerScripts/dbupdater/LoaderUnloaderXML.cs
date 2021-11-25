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
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using System.Reflection;
using System.IO;

using DOL.Database;
using DOL.Database.Attributes;

using log4net;

namespace DOL.GS.DatabaseUpdate
{
	/// <summary>
	/// Tools for Loading and Unloading XML DataObjects.
	/// </summary>
	public static class LoaderUnloaderXML
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// Retrieve All DataTables Type
		/// </summary>
		public static IList<Type> GetAllDataTableTypes()
		{
			var result = new List<Type>();
			try
			{
				// Walk through each assembly in scripts
				foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
				{
					// Walk through each type in the assembly
					foreach (var type in asm.GetTypes())
					{
						if (!type.IsClass || type.IsAbstract || !typeof(DataObject).IsAssignableFrom(type))
							continue;
						
						// Don't Keep Views...
						if (type.GetCustomAttributes<DataTable>(false).Any() && AttributesUtils.GetViewName(type) == null)
							result.Add(type);
					}
				}
			}
			catch (Exception ex)
			{
				if (log.IsErrorEnabled)
					log.Error("Error while Listing Data Tables", ex);
			}
			
			return result;
		}
		
		/// <summary>
		/// Get an XML Serializer for this collection (objectsType) containing Object (t)
		/// Only Match DataElement for Serialization
		/// </summary>
		/// <param name="objectsType">Collection Type</param>
		/// <param name="t">Containing Objects Type</param>
		/// <returns></returns>
		public static XmlSerializer GetXMLSerializer(Type objectsType, Type t)
		{
				// filter XML Attributes
				var xmlOver = new XmlAttributeOverrides();
				var xmlIgnore = new XmlAttributes();
				xmlIgnore.XmlIgnore = true;
				
				// Ignore Fields that are not DataElems
				foreach (var member in t.GetMembers())
				{
					var dataElems = member.GetCustomAttributes<DataElement>(true).ToArray();
					var primaryKeys = member.GetCustomAttributes<PrimaryKey>(true).ToArray();
					if(!dataElems.Any() && (!primaryKeys.Any() || primaryKeys.First().AutoIncrement))
					{
						try
						{
							if (member.MemberType != MemberTypes.Constructor)
							{
								xmlOver.Add(member.DeclaringType, member.Name, xmlIgnore);
								if (member.DeclaringType != t)
									xmlOver.Add(t, member.Name, xmlIgnore);
								
								// Property have weird behavior when overriden...
								var memberProperty = member as PropertyInfo;
								if (memberProperty != null)
								{
									var baseType = memberProperty.GetGetMethod().GetBaseDefinition().DeclaringType;
									if (baseType != t && baseType != member.DeclaringType)
										xmlOver.Add(baseType, member.Name, xmlIgnore);
								}
							}
						}
						catch (Exception e)
						{
							if (log.IsWarnEnabled)
								log.WarnFormat("Errors while Creating XML Serializer (Continuing anyway...): {0}", e);
						}
					}
				}
				
				
				return new XmlSerializer(objectsType, xmlOver);
		}
		
		/// <summary>
		/// Unload a DataObject Type to XML File
		/// </summary>
		/// <param name="t"></param>
		public static void UnloadXMLTable(Type t, string directory)
		{
			if (t == null || !typeof(DataObject).IsAssignableFrom(t))
			{
				if (log.IsInfoEnabled)
					log.InfoFormat("Null Type or Incompatible Type in UnloadXMLTable Call: {0}", Environment.StackTrace);
			}
			
			try
			{
				// Get all object "Method" Through Reflection
				var classMethod = GameServer.Database.GetType().GetMethod("SelectAllObjects", Type.EmptyTypes);
				var genericMethod = classMethod.MakeGenericMethod(t);
				
				// Get Table Name
				var attrib = t.GetCustomAttributes<DataTable>(false);
				if (attrib.Any())
				{
					var tableName = AttributesUtils.GetTableName(t);
					
					var path = string.Format("{0}{1}{2}.xml", directory, Path.DirectorySeparatorChar, tableName);
	       			
					if (log.IsInfoEnabled)
						log.InfoFormat("Unloading Table {0} - To file {1}", tableName, path);
					
					var objects = (IEnumerable)genericMethod.Invoke(GameServer.Database, Array.Empty<object>() );
					
					try
					{
						// Try Sorting them !
						var remarkables = DatabaseUtils.GetRemarkableMembers(t, true);
						
						if (remarkables.Length > 0)
						{
							var firstOrder = objects.Cast<DataObject>().OrderBy(o => remarkables.First().GetValue(o));
							foreach (var remarkable in remarkables.Skip(1))
							{
								var followingOrder = firstOrder.ThenBy(o => remarkable.GetValue(o));
								firstOrder = followingOrder;
							}
							
							// Dynamic cast
							var castMethods = typeof(Enumerable).GetMethod("Cast");
							var castGeneric = castMethods.MakeGenericMethod(t);
							var toArrayMethod = typeof(Enumerable).GetMethod("ToArray");
							var toArrayGeneric = toArrayMethod.MakeGenericMethod(t);
							
							objects = (IEnumerable)toArrayGeneric.Invoke(null, new object[] {castGeneric.Invoke(null , new object[]{firstOrder})});
						}
					}
					catch (Exception oe)
					{
						if (log.IsWarnEnabled)
							log.WarnFormat("Error while sorting Table {0} for XML Unload, Probably Unsorted... - {1}", tableName, oe);
					}
					// Delete Older Files
					if (File.Exists(path))
						File.Delete(path);
					
					// Write XML DataTable
					var serializer = GetXMLSerializer(t.MakeArrayType(), t);
					using (var writer = new StreamWriter(path))
					{
						serializer.Serialize(writer, objects);
					}
				}
			}
			catch (Exception ex)
			{
				if (log.IsWarnEnabled)
					log.WarnFormat("Could Not Unload Table {0}: {1}", t, ex);
			}
		}
		
		/// <summary>
		/// Load an XML File to a DataObject Array
		/// RootElement should be "ArrayOf{DataObject Table Tagged}"
		/// </summary>
		/// <param name="xml">File info of the XML Package</param>
		/// <returns>Array of DataObject</returns>
		public static DataObject[] LoadXMLTableFromFile(FileInfo xml)
		{
			if (!xml.Exists)
				return Array.Empty<DataObject>();
			
			var types = GetAllDataTableTypes();
			
			Type loadingType = null;
			try
			{
				// Read Root of XML File
				var xmldoc = new XmlDocument();
				xmldoc.Load(xml.OpenRead());
				loadingType = types.First(t => t.Name.Equals(xmldoc.DocumentElement.Name.Replace("ArrayOf", "")));
			}
			catch (Exception e)
			{
				if (log.IsErrorEnabled)
					log.ErrorFormat("Could no guess Type of XML Package {0} - {1}", xml.FullName, e);
				
				return Array.Empty<DataObject>();
			}
			
			var serializer = GetXMLSerializer(loadingType.MakeArrayType(), loadingType);
			
			using (var stream = xml.OpenRead())
			{
				using (var reader = new StreamReader(stream))
				{
					return (DataObject[])serializer.Deserialize(reader);
				}
			}
		}
	}
}
