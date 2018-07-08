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
using System.Linq;
using System.Collections.Generic;

namespace DOL.GS
{
    /// <summary>
    /// Allow to handle Custom Params Valuable Interface Member as typed generic imported from string collections.
    /// </summary>
    public static class CustomParamsExtensions
    {
        /// <summary>
        /// Search Params Collection to Extract the Value identified by Key.
        /// From Dictionary&lt;stringKey, List&lt;stringValue&gt;&gt; (List&lt;stringValue&gt;.First() Casted to T)
        /// </summary>
        /// <param name="obj">Custom Params Object</param>
        /// <param name="key">Param Key</param>
        /// <param name="def">Default Value</param>
        /// <returns>Param Value or Default if Key not found</returns>
        public static T GetParamValue<T>(this ICustomParamsValuable obj, string key, T def = default(T))
        {
            if (obj.CustomParamsDictionary == null || obj.CustomParamsDictionary.Count == 0)
            {
                return def;
            }

            // Is key existing ?
            List<string> list;
            if (obj.CustomParamsDictionary.TryGetValue(key, out list))
            {
                try
                {
                    return (T)Convert.ChangeType(obj.CustomParamsDictionary[key].First(), typeof(T));
                }
                catch
                {
                    return def;
                }
            }

            return def;
        }

        /// <summary>
        /// Search Params Collection to Extract the Values Collection identified by Key.
        /// From Dictionary&lt;stringKey, List&lt;stringValue&gt;&gt; (List&lt;stringValue&gt; Casted to List&lt;T&gt;)
        /// </summary>
        /// <param name="obj">Custom Params Object</param>
        /// <param name="key">Param Key</param>
        /// <returns>Params Values collection or empty collection if not found</returns>
        public static IList<T> GetParamValues<T>(this ICustomParamsValuable obj, string key)
        {
            if (obj.CustomParamsDictionary == null || obj.CustomParamsDictionary.Count == 0)
            {
                return new List<T>();
            }

            List<string> list;

            // Is key existing ?
            if (obj.CustomParamsDictionary.TryGetValue(key, out list))
            {
                return list.Select(val => {
                                    try
                                    {
                                        return new Tuple<bool, T>(true, (T)Convert.ChangeType(val, typeof(T)));
                                    }
                                    catch
                                    {
                                        return new Tuple<bool, T>(false, default(T));
                                    }
                                   }).Where(t => t.Item1).Select(t => t.Item2).ToList();
            }

            return new List<T>();
        }

        /// <summary>
        /// Set or Replace Value in Params Collection with given Key
        /// </summary>
        /// <param name="obj">Custom Params Object</param>
        /// <param name="key">Param Key</param>
        /// <param name="value">Object Value</param>
        public static void SetParamValue<T>(this ICustomParamsValuable obj, string key, T value)
        {
            obj.ReplaceEntry(key, new [] { Convert.ToString(value) }.ToList());
        }

        /// <summary>
        /// Set or Replace Values in Params Collection with given Key
        /// </summary>
        /// <param name="obj">Custom Params Object</param>
        /// <param name="key">Param Key</param>
        /// <param name="values">Values Enumerable</param>
        public static void SetParamValues<T>(this ICustomParamsValuable obj, string key, IEnumerable<T> values)
        {
            obj.ReplaceEntry(key, values.Select(v => Convert.ToString(v)).ToList());
        }

        /// <summary>
        /// Add Value in Params Collection with given Key
        /// </summary>
        /// <param name="obj">Custom Params Object</param>
        /// <param name="key">Param Key</param>
        /// <param name="value">Object Value</param>
        public static void AddParamValue<T>(this ICustomParamsValuable obj, string key, T value)
        {
            List<string> list;
            if (!obj.CustomParamsDictionary.TryGetValue(key, out list))
            {
                list = new List<string>();
            }

            obj.ReplaceEntry(key, list.Concat(new [] { Convert.ToString(value) }).ToList());
        }

        /// <summary>
        /// Add Values in Params Collection with given Key
        /// </summary>
        /// <param name="obj">Custom Params Object</param>
        /// <param name="key">Param Key</param>
        /// <param name="values">Values Enumerable</param>
        public static void AddParamValues<T>(this ICustomParamsValuable obj, string key, IEnumerable<T> values)
        {
            List<string> list;
            if (!obj.CustomParamsDictionary.TryGetValue(key, out list))
            {
                list = new List<string>();
            }

            obj.ReplaceEntry(key, list.Concat(values.Select(v => Convert.ToString(v))).ToList());
        }

        /// <summary>
        /// Remove Value in Params Collection with given Key
        /// </summary>
        /// <param name="obj">Custom Params Object</param>
        /// <param name="key">Param Key</param>
        /// <param name="value">Object Value to be removed</param>
        /// <returns>True if Value was removed</returns>
        public static bool RemoveParamValue<T>(this ICustomParamsValuable obj, string key, T value)
        {
            List<string> list;
            if (!obj.CustomParamsDictionary.TryGetValue(key, out list))
            {
                list = new List<string>();
            }

            bool removed = false;

            obj.ReplaceEntry(key, list.Where(val => {
                                                try
                                                {
                                                    var item = (T)Convert.ChangeType(val, typeof(T));

                                                    if (value.Equals(item))
                                                    {
                                                        removed = true;
                                                        return false;
                                                    }
                                                    return true;
                                                }
                                                catch
                                                {
                                                    return true;
                                                }
                                             }).ToList());
            return removed;
        }

        /// <summary>
        /// Remove Value in Params Collection with given Key
        /// </summary>
        /// <param name="obj">Custom Params Object</param>
        /// <param name="key">Param Key</param>
        /// <param name="values">Values Enumerable to be removed</param>
        /// <returns>True if ANY Value was removed</returns>
        public static bool RemoveParamValues<T>(this ICustomParamsValuable obj, string key, IEnumerable<T> values)
        {
            List<string> list;
            if (!obj.CustomParamsDictionary.TryGetValue(key, out list))
            {
                list = new List<string>();
            }

            bool removed = false;

            obj.ReplaceEntry(key, list.Where(val => {
                                                try
                                                {
                                                    var item = (T)Convert.ChangeType(val, typeof(T));

                                                    if (values.Contains(item))
                                                    {
                                                        removed = true;
                                                        return false;
                                                    }
                                                    return true;
                                                }
                                                catch
                                                {
                                                    return true;
                                                }
                                             }).ToList());
            return removed;
        }

        /// <summary>
        /// Remove all Values in Params Collection with given Key
        /// </summary>
        /// <param name="obj">Custom Params Object</param>
        /// <param name="key">Param Key</param>
        public static void RemoveParam(this ICustomParamsValuable obj, string key)
        {
            obj.ReplaceEntry(key, new List<string>());
        }

        /// <summary>
        /// Replace Params Collection Value at given Key
        /// </summary>
        /// <param name="obj">Custom Params Object</param>
        /// <param name="key">Param Key</param>
        /// <param name="val">Values Enumerable to set</param>
        private static void ReplaceEntry(this ICustomParamsValuable obj, string key, List<string> val)
        {
            obj.CustomParamsDictionary =
                obj.CustomParamsDictionary.Select(kv => kv.Key == key ? new KeyValuePair<string, List<string>>(key, val) : kv)
                .ToDictionary(kv => kv.Key, kv => kv.Value);
        }

        /// <summary>
        /// Init Object Params Collection from Source Collection
        /// </summary>
        /// <param name="obj">Custom Params Object</param>
        /// <param name="collection">Source Collection</param>
        /// <param name="key">Key Selector</param>
        /// <param name="value">Value Selector</param>
        public static void InitFromCollection<T>(this ICustomParamsValuable obj, IEnumerable<T> collection, Func<T, string> key, Func<T, string> value)
        {
            if (collection == null)
            {
                obj.CustomParamsDictionary = new Dictionary<string, List<string>>();
                return;
            }

            obj.CustomParamsDictionary = collection.GroupBy(item => key(item))
                .ToDictionary(grp => grp.Key, grp => grp.Select(item => value(item)).ToList());
        }
    }
}
