﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace DotnetSdkUtilities.Extensions.ObjectExtensions
{
    public static class ObjectExtensions
    {
        public static object GetValue(this object instance, string path)
        {
            //maybe [ "a2", "b1", "1]", "3]", "c1" ]
            var pathSplitStr = path.Split(new char[] { '.', '[' });
            Type type = instance.GetType();
            PropertyInfo propInfo;
            foreach (var str in pathSplitStr)
            {
                if (str.Contains("]"))
                {
                    int index;
                    if (int.TryParse(str.Replace("]", ""), out index))
                    {
                        var elementType = type.GetElementType();
                        if (elementType == null)
                            throw new ArgumentException("Properties path is not correct");
                        var array = instance as Array;
                        if (array == null)
                            throw new ArgumentException("Properties path is not correct");
                        instance = array.GetValue(index);
                        type = elementType;
                    }
                    else
                    {
                        throw new Exception("Currently getting values in Array does not yet support using types other than int");
                    }
                }
                else
                {
                    propInfo = type.GetProperty(str);
                    if (propInfo != null)
                    {
                        instance = propInfo.GetValue(instance);
                        type = propInfo.PropertyType;
                    }
                    else throw new ArgumentException("Properties path is not correct");
                }
            }
            return instance;
        }
        public static void SetValue(this object instance, string path, object value)
        {
            //maybe [ "a2", "b1", "1]", "3]", "c1" ]
            var pathSplitStr = path.Split(new char[] { '.', '[' });
            Type type = instance.GetType();
            PropertyInfo propInfo;
            var lastIndex = pathSplitStr.Length - 1;
            for (int i = 0; i < pathSplitStr.Length; i++)
            {
                if (i == lastIndex)
                {
                    if (pathSplitStr[i].Contains("]"))
                    {
                        int index;
                        if (int.TryParse(pathSplitStr[i].Replace("]", ""), out index))
                        {
                            var elementType = type.GetElementType();
                            if (elementType == null)
                                throw new ArgumentException("Properties path is not correct");
                            var array = instance as Array;
                            if (array == null)
                                throw new ArgumentException("Properties path is not correct");
                            array.SetValue(Convert.ChangeType(value, elementType), index);
                        }
                        else
                        {
                            throw new Exception("Currently getting values in Array does not yet support using types other than int");
                        }
                    }
                    else
                    {
                        propInfo = type.GetProperty(pathSplitStr[i]);
                        if (propInfo != null)
                        {
                            propInfo.SetValue(instance, Convert.ChangeType(value, propInfo.PropertyType));
                        }
                        else throw new ArgumentException("Properties path is not correct");
                    }
                }
                else
                {
                    if (pathSplitStr[i].Contains("]"))
                    {
                        int index;
                        if (int.TryParse(pathSplitStr[i].Replace("]", ""), out index))
                        {
                            var elementType = type.GetElementType();
                            if (elementType == null)
                                throw new ArgumentException("Properties path is not correct");
                            var array = instance as Array;
                            if (array == null)
                                throw new ArgumentException("Properties path is not correct");
                            instance = array.GetValue(index);
                            if (instance == null)
                            {
                                //instance = Activator.CreateInstance(propInfo.PropertyType);
                            }
                            type = elementType;
                        }
                        else
                        {
                            throw new Exception("Currently getting values in Array does not yet support using types other than int");
                        }
                    }
                    else
                    {
                        propInfo = type.GetProperty(pathSplitStr[i]);
                        if (propInfo != null)
                        {
                            instance = propInfo.GetValue(instance);
                            if (instance == null)
                            {
                                instance = Activator.CreateInstance(propInfo.PropertyType);
                            }
                            type = propInfo.PropertyType;
                        }
                        else throw new ArgumentException("Properties path is not correct");
                    }
                }
            }
        }

        /// <summary>
        /// Converts a complex object into a cache key string. It can handle complex objects.
        /// </summary>
        /// <param name="obj">The object to be converted into a cache key.</param>
        /// <returns>A string representing the cache key.</returns>
        public static string ToCacheKey(this object obj)
        {
            if (obj == null)
            {
                return "null";
            }

            var type = obj.GetType();
            if (type.IsValueType || obj is string)
            {
                if (type.GetProperty("Key") != null && type.GetProperty("Value") != null)
                {
                    var key = type.GetProperty("Key").GetValue(obj);
                    var value = type.GetProperty("Value").GetValue(obj);
                    return $"{key}_{ToCacheKey(value)}";
                }
                return obj.ToString();
            }

            if (obj is IEnumerable enumerable)
            {
                var enumerableParts = new List<string>();
                foreach (var item in enumerable)
                {
                    enumerableParts.Add(ToCacheKey(item));
                }
                return string.Join(",", enumerableParts);
            }

            if (obj is IDictionary dictionary)
            {
                var dictionaryParts = new List<string>();
                foreach (DictionaryEntry item in dictionary)
                {
                    dictionaryParts.Add($"{item.Key}={ToCacheKey(item.Value)}");
                }
                return string.Join(",", dictionaryParts);
            }

            var properties = type.GetProperties();
            var parts = new List<string>();
            foreach (var property in properties)
            {
                var value = property.GetValue(obj);
                parts.Add($"{property.Name}={ToCacheKey(value)}");
            }

            return string.Join(",", parts);
        }
        /// <summary>
        /// Checks if the object has a property with the specified name.
        /// </summary>
        /// <param name="obj">The object to check.</param>
        /// <param name="propertyName">The name of the property to look for.</param>
        /// <returns>True if the object has a property with the specified name, otherwise false.</returns>
        /// <exception cref="ArgumentNullException">Thrown when obj or propertyName is null.</exception>
        public static bool HasProperty(this object obj, string propertyName)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));
            if (propertyName == null) throw new ArgumentNullException(nameof(propertyName));

            var type = obj.GetType();
            return type.GetProperty(propertyName) != null;
        }

    }
}
