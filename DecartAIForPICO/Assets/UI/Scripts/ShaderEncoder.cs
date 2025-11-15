using System;
using UnityEngine;


/// <summary>Packing methods for encoding data to pass from the CPU to the GPU</summary>
public static unsafe class ShaderEncoder
{
	#region Methods
	#region Public
	/// <summary>
	/// Encodes 2 floats into a single one. 
	/// <b>Only use values between 0 and 1.</b>
	/// <b>There will be a loss in precision.</b>
	/// </summary>
	/// <param name="a">First float.</param>
	/// <param name="b">Second float.</param>
	/// <returns>Packed float with values <paramref name="a"/> and <paramref name="b"/></returns>
	public static float Pack2NormalizedFloats(float a, float b)
	{
		if (a < 0 || a > 1)
			throw new ArgumentOutOfRangeException($"Can only use values for encoding between 0 and 1. Error value: {a}");
		if (b < 0 || b > 1)
			throw new ArgumentOutOfRangeException($"Can only use values for encoding between 0 and 1. Error value: {b}");

		a *= UInt16.MaxValue;
		b *= UInt16.MaxValue;
		uint aInt = (UInt32)Mathf.FloorToInt(a);
		uint bInt = ((UInt32)Mathf.FloorToInt(b)) << 16;
		uint combine = aInt | bInt;

		return UInt32ToSingle(combine);
	}

	/// <summary>
	/// Encodes 2 floats into a single one. 
	/// <b>Only use values between 0 and 1.</b>
	/// <b>There will be a loss in precision.</b>
	/// </summary>
	/// <param name="a">First float.</param>
	/// <param name="b">Second float.</param>
	/// <returns>Packed float with values <paramref name="a"/> and <paramref name="b"/></returns>
	public static (float, float) Unpack2Floats(float encoded)
	{
		uint value = SingleToUInt32(encoded);
		uint aInt = value & 0x0000ffff;
		uint bInt = (value & 0xffff0000) >> 16;

		Vector2 result = new Vector2(aInt, bInt) / 0x0000ffff;
		return (result.x, result.y);
	}
	
	public static float PackColor(Color32 c)
	{
		// Clamp alpha to 254 to avoid creating NaN bit patterns
		// Alpha values of 255 with non-zero RGB create IEEE 754 NaN values
		byte clampedAlpha = (byte)Math.Min((int)c.a, 254);
		byte[] bytes = new byte[4] { c.r, c.g, c.b, clampedAlpha };
		return BitConverter.ToSingle(bytes);
	}
	
	public static Color32 UnpackColor(float c)
	{
		byte[] bytes = BitConverter.GetBytes(c);
		var decodedColor = new Color32(bytes[0], bytes[1], bytes[2], bytes[3]);
		return decodedColor;
	}
	
	#endregion
	
	#region Private
	/// <summary>
	/// Converts <paramref name="value"/> to an unsigned integer.
	/// </summary>
	/// <param name="value">The value to convert.</param>
	/// <returns>The converted value.</returns>
	public static uint SingleToUInt32(float value) => *(uint*)(&value);

	/// <summary>
	/// Converts <paramref name="value"/> to a float.
	/// </summary>
	/// <param name="value">The value to convert.</param>
	/// <returns>The converted value.</returns>
	public static float UInt32ToSingle(uint value) => *(float*)(&value);
	#endregion
	#endregion
}
