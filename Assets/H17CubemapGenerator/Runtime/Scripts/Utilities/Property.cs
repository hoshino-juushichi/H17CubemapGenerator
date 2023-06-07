using System;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

#nullable enable
#pragma warning disable 8604 // Possible null reference argument for parameter '' in ''

namespace Hoshino17
{
	public class PropertyBool
	{
		string? _prefsKey;
		public Action<bool>? onValueChanged;

		public PropertyBool(string? prefsKey, bool def = default)
		{
			_prefsKey = prefsKey;
			_value = def;
			if (!string.IsNullOrEmpty(_prefsKey))
			{
				_value = PropertyKeyValue.LoadBool(_prefsKey, def);
			}
		}

		bool _value;
		public bool value
		{
			get => _value;
			set
			{
				if (_value != value)
				{
					_value = value;
					if (!string.IsNullOrEmpty(_prefsKey))
					{
						PropertyKeyValue.SaveBool(_prefsKey, value);
					}
					onValueChanged?.Invoke(value);
				}
			}
		}
	}

	public class PropertyInt
	{
		string? _prefsKey;
		public Action<int>? onValueChanged;

		public PropertyInt(string? prefsKey, int def = default)
		{
			_prefsKey = prefsKey;
			_value = def;
			if (!string.IsNullOrEmpty(_prefsKey))
			{
				_value = PropertyKeyValue.LoadInt(_prefsKey, def);
			}
		}

		int _value;
		public int value
		{
			get => _value;
			set
			{
				if (_value != value)
				{
					_value = value;
					if (!string.IsNullOrEmpty(_prefsKey))
					{
						PropertyKeyValue.SaveInt(_prefsKey, value);
					}
					onValueChanged?.Invoke(value);
				}
			}
		}
	}

	public class PropertyFloat
	{
		string? _prefsKey;
		public Action<float>? onValueChanged;

		public PropertyFloat(string? prefsKey, float def = default)
		{
			_prefsKey = prefsKey;
			_value = def;
			if (!string.IsNullOrEmpty(_prefsKey))
			{
				_value = PropertyKeyValue.LoadFloat(_prefsKey, def);
			}
		}

		float _value;
		public float value
		{
			get => _value;
			set
			{
				if (_value != value)
				{
					_value = value;
					if (!string.IsNullOrEmpty(_prefsKey))
					{
						PropertyKeyValue.SaveFloat(_prefsKey, value);
					}
					onValueChanged?.Invoke(value);
				}
			}
		}
	}

	public class PropertyString
	{
		string? _prefsKey;
		public Action<string>? onValueChanged;

		public PropertyString(string? prefsKey, string def = "")
		{
			_prefsKey = prefsKey;
			_value = def;
			if (!string.IsNullOrEmpty(_prefsKey))
			{
				_value = PropertyKeyValue.LoadString(_prefsKey, def);
			}
		}

		string _value;
		public string value
		{
			get => _value;
			set
			{
				if (_value != value)
				{
					_value = value;
					if (!string.IsNullOrEmpty(_prefsKey))
					{
						PropertyKeyValue.SaveString(_prefsKey, value);
					}
					onValueChanged?.Invoke(value);
				}
			}
		}
	}

	public class PropertyEnum<T> where T : struct
	{
		string? _prefsKey;
		public Action<T>? onValueChanged;

		public PropertyEnum(string? prefsKey, T def = default(T))
		{
			_prefsKey = prefsKey;
			_value = def;
			if (!string.IsNullOrEmpty(_prefsKey))
			{
				_value = (T)(object)PropertyKeyValue.LoadInt(_prefsKey, (int)(object)def);
			}
		}

		T _value;
		public T value
		{
			get => _value;
			set
			{
				if (!IEquatable<T>.Equals(_value, value))
				{
					_value = value;
					if (!string.IsNullOrEmpty(_prefsKey))
					{
						PropertyKeyValue.SaveInt(_prefsKey, (int)(object)value);
					}
					onValueChanged?.Invoke(value);
				}
			}
		}
	}

	public class PropertyAsset<T> where T : UnityEngine.Object
	{
		string? _prefsKey;
		public Action<T?>? onValueChanged;

		public PropertyAsset(string? prefsKey, T? def = default(T))
		{
			_prefsKey = prefsKey;
			_value = def;
			if (!string.IsNullOrEmpty(_prefsKey))
			{
#if UNITY_EDITOR
				_value = (T)AssetDatabase.LoadAssetAtPath<T>(PropertyKeyValue.LoadString(_prefsKey, string.Empty));
#endif
			}
		}

		T? _value;
		public T? value
		{
			get => _value;
			set
			{
				if (!IEquatable<T>.Equals(_value, value))
				{
					_value = value;
					if (!string.IsNullOrEmpty(_prefsKey))
					{
						string str = string.Empty;
						if (_value != null)
						{
#if UNITY_EDITOR
							str = AssetDatabase.GetAssetPath(_value);
#endif
						}
						PropertyKeyValue.SaveString(_prefsKey, str);
					}
					onValueChanged?.Invoke(value);
				}
			}
		}
	}

	public class PropertyObject<T> where T : UnityEngine.Object
	{
		public Action<T?>? onValueChanged;

		public PropertyObject(T? def = default(T))
		{
			_value = def;
		}

		T? _value;
		public T? value
		{
			get => _value;
			set
			{
				if (!IEquatable<T>.Equals(_value, value))
				{
					_value = value;
					onValueChanged?.Invoke(value);
				}
			}
		}
	}

	public static class PropertyKeyValue
	{
		public static void SaveInt(string key, int value)
		{
			PlayerPrefs.SetInt(key, value);
		}

		public static void SaveFloat(string key, float value)
		{
			PlayerPrefs.SetFloat(key, value);
		}

		public static void SaveBool(string key, bool value)
		{
			PlayerPrefs.SetInt(key, value ? 1 : 0);
		}

		public static void SaveString(string key, string value)
		{
			PlayerPrefs.SetString(key, value);
		}

		public static int LoadInt(string key, int defaultValue)
		{
			return PlayerPrefs.GetInt(key, defaultValue);
		}

		public static float LoadFloat(string key, float defaultValue)
		{
			return PlayerPrefs.GetFloat(key, defaultValue);
		}

		public static bool LoadBool(string key, bool defaultValue)
		{
			return PlayerPrefs.GetInt(key, defaultValue ? 1 : 0) != 0;
		}

		public static string LoadString(string key, string defaultValue)
		{
			return PlayerPrefs.GetString(key, defaultValue);
		}
	}
}
