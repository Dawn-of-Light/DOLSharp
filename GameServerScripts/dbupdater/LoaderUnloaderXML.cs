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
			List<Type> result = new List<Type>();
			try
			{
				// Walk through each assembly in scripts
				foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
				{
					// Walk through each type in the assembly
					foreach (Type type in asm.GetTypes())
					{
						if (type.IsClass != true || !typeof(DataObject).IsAssignableFrom(type))
							continue;
						
						object[] attrib = type.GetCustomAttributes(typeof(DataTable), false);
						if (attrib.Length > 0)
						{
							result.Add(type);
						}
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
				System.Xml.Serialization.XmlAttributeOverrides xmlOver = new XmlAttributeOverrides();
				System.Xml.Serialization.XmlAttributes xmlIgnore = new XmlAttributes();
				xmlIgnore.XmlIgnore = true;
				
				// Ignore Fields that are not DataElems
				foreach (var member in t.GetMembers())
				{
					object[] dataElems = member.GetCustomAttributes(typeof(DataElement), true);
					object[] primaryKeys = member.GetCustomAttributes(typeof(PrimaryKey), true);
					if(dataElems.Length < 1 && (primaryKeys.Length < 1 || ((PrimaryKey)primaryKeys[0]).AutoIncrement))
					{
						try
						{
							if (member.MemberType != MemberTypes.Constructor)
							{
								xmlOver.Add(member.DeclaringType, member.Name, xmlIgnore);
								if (member.DeclaringType != t)
									xmlOver.Add(t, member.Name, xmlIgnore);
								
								// Property have weird behavior when overriden...
								if (member is PropertyInfo)
								{
									Type baseType = ((PropertyInfo)member).GetGetMethod().GetBaseDefinition().DeclaringType;
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
					log.InfoFormat("Null Type or Incompatible Type in UnloadXMLTable Call: {0}", System.Environment.StackTrace);
			}
			
			try
			{
				// Get all object "Method" Through Reflection
				MethodInfo classMethod = GameServer.Database.GetType().GetMethod("SelectAllObjects", Type.EmptyTypes);
				MethodInfo genericMethod = classMethod.MakeGenericMethod(t);
				
				// Get Table Name
				object[] attrib = t.GetCustomAttributes(typeof(DataTable), false);
				if (attrib.Length > 0)
				{
					string tableName = ((DataTable)attrib[0]).TableName;
					
					string path = string.Format("{0}{1}{2}.xml", directory, Path.DirectorySeparatorChar, tableName);
	       			
					if (log.IsInfoEnabled)
						log.InfoFormat("Unloading Table {0} - To file {1}", tableName, path);
					
					IEnumerable objects = (IEnumerable)genericMethod.Invoke(GameServer.Database, new object[]{});
					
					try
					{
						// Try Sorting them !
						IList<MemberInfo> remarkables = GetRemarkableMembers(t, true);
						
						if (remarkables.Count > 0)
						{
							IOrderedEnumerable<object> firstOrder = objects.Cast<object>().OrderBy(o =>
							                                                                       {
							                                                                       	if (remarkables.First() is PropertyInfo)
							                                                            				return ((PropertyInfo)remarkables.First()).GetValue(o, null);
							                                                            			else if (remarkables.First() is FieldInfo)
							                                                            				return ((FieldInfo)remarkables.First()).GetValue(o);
							                                                            	
							                                                            			return null;
							                                                                       });
							foreach (var remarkable in remarkables.Skip(1))
							{
								IOrderedEnumerable<object> followingOrder = firstOrder.ThenBy(o =>
								                                                              {
								                                                            	if (remarkable is PropertyInfo)
								                                                            		return ((PropertyInfo)remarkable).GetValue(o, null);
								                                                            	else if (remarkable is FieldInfo)
								                                                            		return ((FieldInfo)remarkable).GetValue(o);
								                                                            	
								                                                            	return null;
								                                                              });
								
								firstOrder = followingOrder;
							}
							
							// Dynamic cast
							var castMethods = typeof(System.Linq.Enumerable).GetMethod("Cast");
							var castGeneric = castMethods.MakeGenericMethod(t);
							var toArrayMethod = typeof(System.Linq.Enumerable).GetMethod("ToArray");
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
					XmlSerializer serializer = GetXMLSerializer(t.MakeArrayType(), t);
					using (StreamWriter writer = new StreamWriter(path))
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
				return new DataObject[0];
			
			IList<Type> types = GetAllDataTableTypes();
			
			Type loadingType = null;
			try
			{
				// Read Root of XML File
				XmlDocument xmldoc = new XmlDocument();
				xmldoc.Load(xml.OpenRead());
				loadingType = types.Where(t => t.Name.Equals(xmldoc.DocumentElement.Name.Replace("ArrayOf", ""))).First();
			}
			catch (Exception e)
			{
				if (log.IsErrorEnabled)
					log.ErrorFormat("Could no guess Type of XML Package {0} - {1}", xml.FullName, e);
				
				return new DataObject[0];
			}
			
			XmlSerializer serializer = GetXMLSerializer(loadingType.MakeArrayType(), loadingType);
			
			using (FileStream stream = xml.OpenRead())
			{
				using (StreamReader reader = new StreamReader(stream))
				{
					return (DataObject[])serializer.Deserialize(reader);
				}
			}
		}
		
		/// <summary>
		/// List all Unique Members of a DataTable, Using them for duplicate Matching.
		/// </summary>
		/// <param name="xmlType">DataObject Type</param>
		/// <param name="primaryKey">Primary Key Reference if Available</param>
		/// <returns>List of MemberInfo having Unique Attributes</returns>
		public static IList<MemberInfo> GetUniqueMembers(Type xmlType, out MemberInfo primaryKey)
		{
			// Find unique Fields
			List<MemberInfo> uniqueMember = new List<MemberInfo>();
			primaryKey = null;
			foreach(MemberInfo member in xmlType.GetMembers())
			{
				object[] attrs = member.GetCustomAttributes(typeof(DataElement), true);
				if (attrs.Length > 0)
				{
					if (((DataElement)attrs[0]).Unique)
						uniqueMember.Add(member);
				}
				
				attrs = member.GetCustomAttributes(typeof(PrimaryKey), true);
				if (attrs.Length > 0)
				{
					if (!((PrimaryKey)attrs[0]).AutoIncrement)
						uniqueMember.Add(member);
					primaryKey = member;
				}
			}

			return uniqueMember;
		}
		
		/// <summary>
		/// List Remarkable Members of a DataTable, Using them for Default Ordering
		/// This includes non-generated Primary Key, Unique Field, and Indexed Fields (optionnaly)
		/// </summary>
		/// <param name="xmlType">DataObject Type</param>
		/// <param name="forceIndexes">Returns Indexes even if enough Unique type have been gathered</param>
		/// <returns>List of Remkarkable MemberInfo of given DataObject</returns>
		public static IList<MemberInfo> GetRemarkableMembers(Type xmlType, bool forceIndexes)
		{
			// Find unique Fields
			MemberInfo dummy;
			IList<MemberInfo> remarkableMember = GetUniqueMembers(xmlType, out dummy);
			
			// We don't have enough fields, Try indexes !
			if (remarkableMember.Count < 1 || forceIndexes)
			{
				foreach(MemberInfo member in xmlType.GetMembers())
				{
					object[] attrs = member.GetCustomAttributes(typeof(DataElement), true);
					if (attrs.Length > 0)
					{
						if (((DataElement)attrs[0]).Index)
						{
							remarkableMember.Add(member);
							
							// Check if this is a multiple column index
							string[] columns = ((DataElement)attrs[0]).IndexColumns.Split(',');
							foreach(string col in columns)
							{
								MemberInfo[] index = xmlType.GetMember(col);
								if (index.Length > 0 && !remarkableMember.Contains(index[0]))
									remarkableMember.Add(index[0]);
							}
						}
					}
				}
			}

			return remarkableMember;
		}
	}
}
