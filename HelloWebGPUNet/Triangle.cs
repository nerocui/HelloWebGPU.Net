﻿using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Runtime.InteropServices;
using System.Text;
using WaveEngine.Bindings.WebGPU;

namespace HelloWebGPUNet
{
	using WGPUDevice = IntPtr;
	using WGPUShaderModule = IntPtr;

	public unsafe class Triangle
	{
		public static void CreatePipelineAndBuffers(WGPUDevice device)
        {
			// Load shaders
			ShaderCodeToUnmanagedMemory(triangleVert, out triangle_vert);
			ShaderCodeToUnmanagedMemory(triangleFrag, out triangle_frag);
			WGPUShaderModule vertMod = CreateShader(device, (uint*)triangle_vert.ToPointer(), (uint)triangleVert.Length);
			WGPUShaderModule fragMod = CreateShader(device, (uint*)triangle_frag.ToPointer(), (uint)triangleFrag.Length);
		}

		private static void ShaderCodeToUnmanagedMemory(UInt32[] code, out IntPtr p_code)
        {
			int byteCount = code.Length * sizeof(UInt32);
			p_code = Marshal.AllocHGlobal(byteCount);
			// auxiliar byte array is needed because marshal does not accept UInt
			byte[] byte_code = new byte[byteCount];
			Buffer.BlockCopy(code, 0, byte_code, 0, byteCount);
			Marshal.Copy(byte_code, 0, p_code, byteCount);
		}

		/**
		 * Helper to create a shader from SPIR-V IR.
		 *
		 * \param[in] code shader source (output using the \c -V \c -x options in \c glslangValidator)
		 * \param[in] size size of \a code in bytes
		 * \param[in] label optional shader name
		 */
		private static WGPUShaderModule CreateShader(WGPUDevice device, uint* code, UInt32 size, char* label = null)
        {
			WGPUShaderModuleSPIRVDescriptor spirv = new WGPUShaderModuleSPIRVDescriptor()
			{
				chain = new WGPUChainedStruct()
				{
					sType = WGPUSType.WGPUSType_ShaderModuleSPIRVDescriptor
				},
				codeSize = size,
				code = code
			};

			WGPUShaderModuleDescriptor desc = new WGPUShaderModuleDescriptor()
			{
				nextInChain = (WGPUChainedStruct*)&spirv,
				label = label
			};

			return WebGPUNative.wgpuDeviceCreateShaderModule(device, &desc);
		}

		/**
		 * Vertex shader SPIR-V.
		 * \code
		 *	// glslc -Os -mfmt=num -o - -c in.vert
		 *	#version 450
		 *	layout(set = 0, binding = 0) uniform Rotation {
		 *		float uRot;
		 *	};
		 *	layout(location = 0) in  vec2 aPos;
		 *	layout(location = 1) in  vec3 aCol;
		 *	layout(location = 0) out vec3 vCol;
		 *	void main() {
		 *		float cosA = cos(radians(uRot));
		 *		float sinA = sin(radians(uRot));
		 *		mat3 rot = mat3(cosA, sinA, 0.0,
		 *					   -sinA, cosA, 0.0,
		 *						0.0,  0.0,  1.0);
		 *		gl_Position = vec4(rot * vec3(aPos, 1.0), 1.0);
		 *		vCol = aCol;
		 *	}
		 * \endcode
		 */
		private static IntPtr triangle_vert;
		private static UInt32[] triangleVert = {
			0x07230203, 0x00010000, 0x000d0008, 0x00000043, 0x00000000, 0x00020011, 0x00000001, 0x0006000b,
			0x00000001, 0x4c534c47, 0x6474732e, 0x3035342e, 0x00000000, 0x0003000e, 0x00000000, 0x00000001,
			0x0009000f, 0x00000000, 0x00000004, 0x6e69616d, 0x00000000, 0x0000002d, 0x00000031, 0x0000003e,
			0x00000040, 0x00050048, 0x00000009, 0x00000000, 0x00000023, 0x00000000, 0x00030047, 0x00000009,
			0x00000002, 0x00040047, 0x0000000b, 0x00000022, 0x00000000, 0x00040047, 0x0000000b, 0x00000021,
			0x00000000, 0x00050048, 0x0000002b, 0x00000000, 0x0000000b, 0x00000000, 0x00050048, 0x0000002b,
			0x00000001, 0x0000000b, 0x00000001, 0x00050048, 0x0000002b, 0x00000002, 0x0000000b, 0x00000003,
			0x00050048, 0x0000002b, 0x00000003, 0x0000000b, 0x00000004, 0x00030047, 0x0000002b, 0x00000002,
			0x00040047, 0x00000031, 0x0000001e, 0x00000000, 0x00040047, 0x0000003e, 0x0000001e, 0x00000000,
			0x00040047, 0x00000040, 0x0000001e, 0x00000001, 0x00020013, 0x00000002, 0x00030021, 0x00000003,
			0x00000002, 0x00030016, 0x00000006, 0x00000020, 0x0003001e, 0x00000009, 0x00000006, 0x00040020,
			0x0000000a, 0x00000002, 0x00000009, 0x0004003b, 0x0000000a, 0x0000000b, 0x00000002, 0x00040015,
			0x0000000c, 0x00000020, 0x00000001, 0x0004002b, 0x0000000c, 0x0000000d, 0x00000000, 0x00040020,
			0x0000000e, 0x00000002, 0x00000006, 0x00040017, 0x00000018, 0x00000006, 0x00000003, 0x00040018,
			0x00000019, 0x00000018, 0x00000003, 0x0004002b, 0x00000006, 0x0000001e, 0x00000000, 0x0004002b,
			0x00000006, 0x00000022, 0x3f800000, 0x00040017, 0x00000027, 0x00000006, 0x00000004, 0x00040015,
			0x00000028, 0x00000020, 0x00000000, 0x0004002b, 0x00000028, 0x00000029, 0x00000001, 0x0004001c,
			0x0000002a, 0x00000006, 0x00000029, 0x0006001e, 0x0000002b, 0x00000027, 0x00000006, 0x0000002a,
			0x0000002a, 0x00040020, 0x0000002c, 0x00000003, 0x0000002b, 0x0004003b, 0x0000002c, 0x0000002d,
			0x00000003, 0x00040017, 0x0000002f, 0x00000006, 0x00000002, 0x00040020, 0x00000030, 0x00000001,
			0x0000002f, 0x0004003b, 0x00000030, 0x00000031, 0x00000001, 0x00040020, 0x0000003b, 0x00000003,
			0x00000027, 0x00040020, 0x0000003d, 0x00000003, 0x00000018, 0x0004003b, 0x0000003d, 0x0000003e,
			0x00000003, 0x00040020, 0x0000003f, 0x00000001, 0x00000018, 0x0004003b, 0x0000003f, 0x00000040,
			0x00000001, 0x0006002c, 0x00000018, 0x00000042, 0x0000001e, 0x0000001e, 0x00000022, 0x00050036,
			0x00000002, 0x00000004, 0x00000000, 0x00000003, 0x000200f8, 0x00000005, 0x00050041, 0x0000000e,
			0x0000000f, 0x0000000b, 0x0000000d, 0x0004003d, 0x00000006, 0x00000010, 0x0000000f, 0x0006000c,
			0x00000006, 0x00000011, 0x00000001, 0x0000000b, 0x00000010, 0x0006000c, 0x00000006, 0x00000012,
			0x00000001, 0x0000000e, 0x00000011, 0x0006000c, 0x00000006, 0x00000017, 0x00000001, 0x0000000d,
			0x00000011, 0x0004007f, 0x00000006, 0x00000020, 0x00000017, 0x00060050, 0x00000018, 0x00000023,
			0x00000012, 0x00000017, 0x0000001e, 0x00060050, 0x00000018, 0x00000024, 0x00000020, 0x00000012,
			0x0000001e, 0x00060050, 0x00000019, 0x00000026, 0x00000023, 0x00000024, 0x00000042, 0x0004003d,
			0x0000002f, 0x00000032, 0x00000031, 0x00050051, 0x00000006, 0x00000033, 0x00000032, 0x00000000,
			0x00050051, 0x00000006, 0x00000034, 0x00000032, 0x00000001, 0x00060050, 0x00000018, 0x00000035,
			0x00000033, 0x00000034, 0x00000022, 0x00050091, 0x00000018, 0x00000036, 0x00000026, 0x00000035,
			0x00050051, 0x00000006, 0x00000037, 0x00000036, 0x00000000, 0x00050051, 0x00000006, 0x00000038,
			0x00000036, 0x00000001, 0x00050051, 0x00000006, 0x00000039, 0x00000036, 0x00000002, 0x00070050,
			0x00000027, 0x0000003a, 0x00000037, 0x00000038, 0x00000039, 0x00000022, 0x00050041, 0x0000003b,
			0x0000003c, 0x0000002d, 0x0000000d, 0x0003003e, 0x0000003c, 0x0000003a, 0x0004003d, 0x00000018,
			0x00000041, 0x00000040, 0x0003003e, 0x0000003e, 0x00000041, 0x000100fd, 0x00010038
		};

		/**
		 * Fragment shader SPIR-V.
		 * \code
		 *	// glslc -Os -mfmt=num -o - -c in.frag
		 *	#version 450
		 *	layout(location = 0) in  vec3 vCol;
		 *	layout(location = 0) out vec4 fragColor;
		 *	void main() {
		 *		fragColor = vec4(vCol, 1.0);
		 *	}
		 * \endcode
		 */
		private static IntPtr triangle_frag;
		private static UInt32[] triangleFrag = {
			0x07230203, 0x00010000, 0x000d0007, 0x00000013, 0x00000000, 0x00020011, 0x00000001, 0x0006000b,
			0x00000001, 0x4c534c47, 0x6474732e, 0x3035342e, 0x00000000, 0x0003000e, 0x00000000, 0x00000001,
			0x0007000f, 0x00000004, 0x00000004, 0x6e69616d, 0x00000000, 0x00000009, 0x0000000c, 0x00030010,
			0x00000004, 0x00000007, 0x00040047, 0x00000009, 0x0000001e, 0x00000000, 0x00040047, 0x0000000c,
			0x0000001e, 0x00000000, 0x00020013, 0x00000002, 0x00030021, 0x00000003, 0x00000002, 0x00030016,
			0x00000006, 0x00000020, 0x00040017, 0x00000007, 0x00000006, 0x00000004, 0x00040020, 0x00000008,
			0x00000003, 0x00000007, 0x0004003b, 0x00000008, 0x00000009, 0x00000003, 0x00040017, 0x0000000a,
			0x00000006, 0x00000003, 0x00040020, 0x0000000b, 0x00000001, 0x0000000a, 0x0004003b, 0x0000000b,
			0x0000000c, 0x00000001, 0x0004002b, 0x00000006, 0x0000000e, 0x3f800000, 0x00050036, 0x00000002,
			0x00000004, 0x00000000, 0x00000003, 0x000200f8, 0x00000005, 0x0004003d, 0x0000000a, 0x0000000d,
			0x0000000c, 0x00050051, 0x00000006, 0x0000000f, 0x0000000d, 0x00000000, 0x00050051, 0x00000006,
			0x00000010, 0x0000000d, 0x00000001, 0x00050051, 0x00000006, 0x00000011, 0x0000000d, 0x00000002,
			0x00070050, 0x00000007, 0x00000012, 0x0000000f, 0x00000010, 0x00000011, 0x0000000e, 0x0003003e,
			0x00000009, 0x00000012, 0x000100fd, 0x00010038
		};
	}
}