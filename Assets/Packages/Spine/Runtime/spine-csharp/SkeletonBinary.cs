/******************************************************************************
 * Spine Runtimes License Agreement
 * Last updated April 5, 2025. Replaces all prior versions.
 *
 * Copyright (c) 2013-2025, Esoteric Software LLC
 *
 * Integration of the Spine Runtimes into software or otherwise creating
 * derivative works of the Spine Runtimes is permitted under the terms and
 * conditions of Section 2 of the Spine Editor License Agreement:
 * http://esotericsoftware.com/spine-editor-license
 *
 * Otherwise, it is permitted to integrate the Spine Runtimes into software
 * or otherwise create derivative works of the Spine Runtimes (collectively,
 * "Products"), provided that each user of the Products must obtain their own
 * Spine Editor license and redistribution of the Products in any form must
 * include this license and copyright notice.
 *
 * THE SPINE RUNTIMES ARE PROVIDED BY ESOTERIC SOFTWARE LLC "AS IS" AND ANY
 * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL ESOTERIC SOFTWARE LLC BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES,
 * BUSINESS INTERRUPTION, OR LOSS OF USE, DATA, OR PROFITS) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
 * THE SPINE RUNTIMES, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

#if (UNITY_5 || UNITY_5_3_OR_NEWER || UNITY_WSA || UNITY_WP8 || UNITY_WP8_1)
#define IS_UNITY
#endif

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;

#if WINDOWS_STOREAPP
using System.Threading.Tasks;
using Windows.Storage;
#endif

namespace Spine {
	public class SkeletonBinary : SkeletonLoader {
		public const int BONE_ROTATE = 0;
		public const int BONE_TRANSLATE = 1;
		public const int BONE_TRANSLATEX = 2;
		public const int BONE_TRANSLATEY = 3;
		public const int BONE_SCALE = 4;
		public const int BONE_SCALEX = 5;
		public const int BONE_SCALEY = 6;
		public const int BONE_SHEAR = 7;
		public const int BONE_SHEARX = 8;
		public const int BONE_SHEARY = 9;
		public const int BONE_INHERIT = 10;

		public const int SLOT_ATTACHMENT = 0;
		public const int SLOT_RGBA = 1;
		public const int SLOT_RGB = 2;
		public const int SLOT_RGBA2 = 3;
		public const int SLOT_RGB2 = 4;
		public const int SLOT_ALPHA = 5;

		public const int ATTACHMENT_DEFORM = 0;
		public const int ATTACHMENT_SEQUENCE = 1;

		public const int PATH_POSITION = 0;
		public const int PATH_SPACING = 1;
		public const int PATH_MIX = 2;

		public const int PHYSICS_INERTIA = 0;
		public const int PHYSICS_STRENGTH = 1;
		public const int PHYSICS_DAMPING = 2;
		public const int PHYSICS_MASS = 4;
		public const int PHYSICS_WIND = 5;
		public const int PHYSICS_GRAVITY = 6;
		public const int PHYSICS_MIX = 7;
		public const int PHYSICS_RESET = 8;

		public const int CURVE_LINEAR = 0;
		public const int CURVE_STEPPED = 1;
		public const int CURVE_BEZIER = 2;

		private readonly List<LinkedMesh> linkedMeshes = new List<LinkedMesh>();

		public SkeletonBinary (AttachmentLoader attachmentLoader)
			: base(attachmentLoader) {
		}

		public SkeletonBinary (params Atlas[] atlasArray)
			: base(atlasArray) {
		}

#if !ISUNITY && WINDOWS_STOREAPP
		private async Task<SkeletonData> ReadFile(string path) {
			var folder = Windows.ApplicationModel.Package.Current.InstalledLocation;
			using (BufferedStream input = new BufferedStream(await folder.GetFileAsync(path).AsTask().ConfigureAwait(false))) {
				SkeletonData skeletonData = ReadSkeletonData(input);
				skeletonData.Name = Path.GetFileNameWithoutExtension(path);
				return skeletonData;
			}
		}

		public override SkeletonData ReadSkeletonData (string path) {
			return this.ReadFile(path).Result;
		}
#else
		public override SkeletonData ReadSkeletonData (string path) {
#if WINDOWS_PHONE
			using (BufferedStream input = new BufferedStream(Microsoft.Xna.Framework.TitleContainer.OpenStream(path))) {
#else
			using (FileStream input = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read)) {
#endif
				SkeletonData skeletonData = ReadSkeletonData(input);
				skeletonData.name = Path.GetFileNameWithoutExtension(path);
				return skeletonData;
			}
		}
#endif // WINDOWS_STOREAPP

		/// <summary>Returns the version string of binary skeleton data.</summary>
		public static string GetVersionString (Stream file) {
			if (file == null) throw new ArgumentNullException("file");

			SkeletonInput input = new SkeletonInput(file);
			return input.GetVersionString();
		}

		public SkeletonData ReadSkeletonData (Stream file) {
			if (file == null) throw new ArgumentNullException("file");
			float scale = this.scale;

			SkeletonData skeletonData = new SkeletonData();
			SkeletonInput input = new SkeletonInput(file);

			long hash = input.ReadLong();
			skeletonData.hash = hash == 0 ? null : hash.ToString();
			skeletonData.version = input.ReadString();
			if (skeletonData.version.Length == 0) skeletonData.version = null;
			// early return for old 3.8 format instead of reading past the end
			if (skeletonData.version.Length > 13) return null;
			skeletonData.x = input.ReadFloat();
			skeletonData.y = input.ReadFloat();
			skeletonData.width = input.ReadFloat();
			skeletonData.height = input.ReadFloat();
			skeletonData.referenceScale = input.ReadFloat() * scale;

			bool nonessential = input.ReadBoolean();

			if (nonessential) {
				skeletonData.fps = input.ReadFloat();

				skeletonData.imagesPath = input.ReadString();
				if (string.IsNullOrEmpty(skeletonData.imagesPath)) skeletonData.imagesPath = null;

				skeletonData.audioPath = input.ReadString();
				if (string.IsNullOrEmpty(skeletonData.audioPath)) skeletonData.audioPath = null;
			}

			int n;
			Object[] o;

			// Strings.
			o = input.strings = new String[n = input.ReadInt(true)];
			for (int i = 0; i < n; i++)
				o[i] = input.ReadString();

			// Bones.
			BoneData[] bones = skeletonData.bones.Resize(n = input.ReadInt(true)).Items;
			for (int i = 0; i < n; i++) {
				String name = input.ReadString();
				BoneData parent = i == 0 ? null : bones[input.ReadInt(true)];
				BoneData data = new BoneData(i, name, parent);
				data.rotation = input.ReadFloat();
				data.x = input.ReadFloat() * scale;
				data.y = input.ReadFloat() * scale;
				data.scaleX = input.ReadFloat();
				data.scaleY = input.ReadFloat();
				data.shearX = input.ReadFloat();
				data.shearY = input.ReadFloat();
				data.Length = input.ReadFloat() * scale;
				data.inherit = InheritEnum.Values[input.ReadInt(true)];
				data.skinRequired = input.ReadBoolean();
				if (nonessential) { // discard non-essential data
					input.ReadInt(); // Color.rgba8888ToColor(data.color, input.readInt());
					input.ReadString(); // data.icon = input.readString();
					input.ReadBoolean(); // data.visible = input.readBoolean();
				}
				bones[i] = data;
			}

			// Slots.
			SlotData[] slots = skeletonData.slots.Resize(n = input.ReadInt(true)).Items;
			for (int i = 0; i < n; i++) {
				String slotName = input.ReadString();

				BoneData boneData = bones[input.ReadInt(true)];
				SlotData slotData = new SlotData(i, slotName, boneData);
				int color = input.ReadInt();
				slotData.r = ((color & 0xff000000) >> 24) / 255f;
				slotData.g = ((color & 0x00ff0000) >> 16) / 255f;
				slotData.b = ((color & 0x0000ff00) >> 8) / 255f;
				slotData.a = ((color & 0x000000ff)) / 255f;

				int darkColor = input.ReadInt(); // 0x00rrggbb
				if (darkColor != -1) {
					slotData.hasSecondColor = true;
					slotData.r2 = ((darkColor & 0x00ff0000) >> 16) / 255f;
					slotData.g2 = ((darkColor & 0x0000ff00) >> 8) / 255f;
					slotData.b2 = ((darkColor & 0x000000ff)) / 255f;
				}

				slotData.attachmentName = input.ReadStringRef();
				slotData.blendMode = (BlendMode)input.ReadInt(true);
				if (nonessential) {
					input.ReadBoolean(); // data.visible = input.readBoolean(); data.path = path;
				}
				slots[i] = slotData;
			}

			// IK constraints.
			o = skeletonData.ikConstraints.Resize(n = input.ReadInt(true)).Items;
			for (int i = 0, nn; i < n; i++) {
				IkConstraintData data = new IkConstraintData(input.ReadString());
				data.order = input.ReadInt(true);
				BoneData[] constraintBones = data.bones.Resize(nn = input.ReadInt(true)).Items;
				for (int ii = 0; ii < nn; ii++)
					constraintBones[ii] = bones[input.ReadInt(true)];
				data.target = bones[input.ReadInt(true)];
				int flags = input.Read();
				data.skinRequired = (flags & 1) != 0;
				data.bendDirection = (flags & 2) != 0 ? 1 : -1;
				data.compress = (flags & 4) != 0;
				data.stretch = (flags & 8) != 0;
				data.uniform = (flags & 16) != 0;
				if ((flags & 32) != 0) data.mix = (flags & 64) != 0 ? input.ReadFloat() : 1;
				if ((flags & 128) != 0) data.softness = input.ReadFloat() * scale;
				o[i] = data;
			}

			// Transform constraints.
			o = skeletonData.transformConstraints.Resize(n = input.ReadInt(true)).Items;
			for (int i = 0, nn; i < n; i++) {
				TransformConstraintData data = new TransformConstraintData(input.ReadString());
				data.order = input.ReadInt(true);
				BoneData[] constraintBones = data.bones.Resize(nn = input.ReadInt(true)).Items;
				for (int ii = 0; ii < nn; ii++)
					constraintBones[ii] = bones[input.ReadInt(true)];
				data.target = bones[input.ReadInt(true)];
				int flags = input.Read();
				data.skinRequired = (flags & 1) != 0;
				data.local = (flags & 2) != 0;
				data.relative = (flags & 4) != 0;
				if ((flags & 8) != 0) data.offsetRotation = input.ReadFloat();
				if ((flags & 16) != 0) data.offsetX = input.ReadFloat() * scale;
				if ((flags & 32) != 0) data.offsetY = input.ReadFloat() * scale;
				if ((flags & 64) != 0) data.offsetScaleX = input.ReadFloat();
				if ((flags & 128) != 0) data.offsetScaleY = input.ReadFloat();
				flags = input.Read();
				if ((flags & 1) != 0) data.offsetShearY = input.ReadFloat();
				if ((flags & 2) != 0) data.mixRotate = input.ReadFloat();
				if ((flags & 4) != 0) data.mixX = input.ReadFloat();
				if ((flags & 8) != 0) data.mixY = input.ReadFloat();
				if ((flags & 16) != 0) data.mixScaleX = input.ReadFloat();
				if ((flags & 32) != 0) data.mixScaleY = input.ReadFloat();
				if ((flags & 64) != 0) data.mixShearY = input.ReadFloat();
				o[i] = data;
			}

			// Path constraints
			o = skeletonData.pathConstraints.Resize(n = input.ReadInt(true)).Items;
			for (int i = 0, nn; i < n; i++) {
				PathConstraintData data = new PathConstraintData(input.ReadString());
				data.order = input.ReadInt(true);
				data.skinRequired = input.ReadBoolean();
				BoneData[] constraintBones = data.bones.Resize(nn = input.ReadInt(true)).Items;
				for (int ii = 0; ii < nn; ii++)
					constraintBones[ii] = bones[input.ReadInt(true)];
				data.target = slots[input.ReadInt(true)];
				int flags = input.Read();
				data.positionMode = (PositionMode)Enum.GetValues(typeof(PositionMode)).GetValue(flags & 1);
				data.spacingMode = (SpacingMode)Enum.GetValues(typeof(SpacingMode)).GetValue((flags >> 1) & 3);
				data.rotateMode = (RotateMode)Enum.GetValues(typeof(RotateMode)).GetValue((flags >> 3) & 3);
				if ((flags & 128) != 0) data.offsetRotation = input.ReadFloat();

				data.position = input.ReadFloat();
				if (data.positionMode == PositionMode.Fixed) data.position *= scale;
				data.spacing = input.ReadFloat();
				if (data.spacingMode == SpacingMode.Length || data.spacingMode == SpacingMode.Fixed) data.spacing *= scale;
				data.mixRotate = input.ReadFloat();
				data.mixX = input.ReadFloat();
				data.mixY = input.ReadFloat();
				o[i] = data;
			}

			// Physics constraints.
			o = skeletonData.physicsConstraints.Resize(n = input.ReadInt(true)).Items;
			for (int i = 0; i < n; i++) {
				PhysicsConstraintData data = new PhysicsConstraintData(input.ReadString());
				data.order = input.ReadInt(true);
				data.bone = bones[input.ReadInt(true)];
				int flags = input.Read();
				data.skinRequired = (flags & 1) != 0;
				if ((flags & 2) != 0) data.x = input.ReadFloat();
				if ((flags & 4) != 0) data.y = input.ReadFloat();
				if ((flags & 8) != 0) data.rotate = input.ReadFloat();
				if ((flags & 16) != 0) data.scaleX = input.ReadFloat();
				if ((flags & 32) != 0) data.shearX = input.ReadFloat();
				data.limit = ((flags & 64) != 0 ? input.ReadFloat() : 5000) * scale;
				data.step = 1f / input.ReadUByte();
				data.inertia = input.ReadFloat();
				data.strength = input.ReadFloat();
				data.damping = input.ReadFloat();
				data.massInverse = (flags & 128) != 0 ? input.ReadFloat() : 1;
				data.wind = input.ReadFloat();
				data.gravity = input.ReadFloat();
				flags = input.Read();
				if ((flags & 1) != 0) data.inertiaGlobal = true;
				if ((flags & 2) != 0) data.strengthGlobal = true;
				if ((flags & 4) != 0) data.dampingGlobal = true;
				if ((flags & 8) != 0) data.massGlobal = true;
				if ((flags & 16) != 0) data.windGlobal = true;
				if ((flags & 32) != 0) data.gravityGlobal = true;
				if ((flags & 64) != 0) data.mixGlobal = true;
				data.mix = (flags & 128) != 0 ? input.ReadFloat() : 1;
				o[i] = data;
			}

			// Default skin.
			Skin defaultSkin = ReadSkin(input, skeletonData, true, nonessential);
			if (defaultSkin != null) {
				skeletonData.defaultSkin = defaultSkin;
				skeletonData.skins.Add(defaultSkin);
			}

			// Skins.
			{
				int i = skeletonData.skins.Count;
				o = skeletonData.skins.Resize(n = i + input.ReadInt(true)).Items;
				for (; i < n; i++)
					o[i] = ReadSkin(input, skeletonData, false, nonessential);
			}

			// Linked meshes.
			n = linkedMeshes.Count;
			for (int i = 0; i < n; i++) {
				LinkedMesh linkedMesh = linkedMeshes[i];
				Skin skin = skeletonData.skins.Items[linkedMesh.skinIndex];
				Attachment parent = skin.GetAttachment(linkedMesh.slotIndex, linkedMesh.parent);
				if (parent == null) throw new Exception("Parent mesh not found: " + linkedMesh.parent);
				linkedMesh.mesh.TimelineAttachment = linkedMesh.inheritTimelines ? (VertexAttachment)parent : linkedMesh.mesh;
				linkedMesh.mesh.ParentMesh = (MeshAttachment)parent;
				if (linkedMesh.mesh.Sequence == null) linkedMesh.mesh.UpdateRegion();
			}
			linkedMeshes.Clear();

			// Events.
			o = skeletonData.events.Resize(n = input.ReadInt(true)).Items;
			for (int i = 0; i < n; i++) {
				EventData data = new EventData(input.ReadString());
				data.Int = input.ReadInt(false);
				data.Float = input.ReadFloat();
				data.String = input.ReadString();
				data.AudioPath = input.ReadString();
				if (data.AudioPath != null) {
					data.Volume = input.ReadFloat();
					data.Balance = input.ReadFloat();
				}
				o[i] = data;
			}

			// Animations.
			o = skeletonData.animations.Resize(n = input.ReadInt(true)).Items;
			for (int i = 0; i < n; i++)
				o[i] = ReadAnimation(input.ReadString(), input, skeletonData);

			return skeletonData;
		}

		/// <returns>May be null.</returns>
		private Skin ReadSkin (SkeletonInput input, SkeletonData skeletonData, bool defaultSkin, bool nonessential) {

			Skin skin;
			int slotCount;

			if (defaultSkin) {
				slotCount = input.ReadInt(true);
				if (slotCount == 0) return null;
				skin = new Skin("default");
			} else {
				skin = new Skin(input.ReadString());

				if (nonessential) input.ReadInt(); // discard, Color.rgba8888ToColor(skin.color, input.readInt());

				Object[] bones = skin.bones.Resize(input.ReadInt(true)).Items;
				BoneData[] bonesItems = skeletonData.bones.Items;
				for (int i = 0, n = skin.bones.Count; i < n; i++)
					bones[i] = bonesItems[input.ReadInt(true)];

				IkConstraintData[] ikConstraintsItems = skeletonData.ikConstraints.Items;
				for (int i = 0, n = input.ReadInt(true); i < n; i++)
					skin.constraints.Add(ikConstraintsItems[input.ReadInt(true)]);
				TransformConstraintData[] transformConstraintsItems = skeletonData.transformConstraints.Items;
				for (int i = 0, n = input.ReadInt(true); i < n; i++)
					skin.constraints.Add(transformConstraintsItems[input.ReadInt(true)]);
				PathConstraintData[] pathConstraintsItems = skeletonData.pathConstraints.Items;
				for (int i = 0, n = input.ReadInt(true); i < n; i++)
					skin.constraints.Add(pathConstraintsItems[input.ReadInt(true)]);
				PhysicsConstraintData[] physicsConstraintsItems = skeletonData.physicsConstraints.Items;
				for (int i = 0, n = input.ReadInt(true); i < n; i++)
					skin.constraints.Add(physicsConstraintsItems[input.ReadInt(true)]);
				skin.constraints.TrimExcess();

				slotCount = input.ReadInt(true);
			}
			for (int i = 0; i < slotCount; i++) {
				int slotIndex = input.ReadInt(true);
				for (int ii = 0, nn = input.ReadInt(true); ii < nn; ii++) {
					String name = input.ReadStringRef();
					Attachment attachment = ReadAttachment(input, skeletonData, skin, slotIndex, name, nonessential);
					if (attachment != null) skin.SetAttachment(slotIndex, name, attachment);
				}
			}
			return skin;
		}

		private Attachment ReadAttachment (SkeletonInput input, SkeletonData skeletonData, Skin skin, int slotIndex,
			String attachmentName, bool nonessential) {
			float scale = this.scale;

			int flags = input.ReadUByte();
			string name = (flags & 8) != 0 ? input.ReadStringRef() : attachmentName;

			switch ((AttachmentType)(flags & 0x7)) { // 0b111
			case AttachmentType.Region: {
				string path = (flags & 16) != 0 ? input.ReadStringRef() : null;
				uint color = (flags & 32) != 0 ? (uint)input.ReadInt() : 0xffffffff;
				Sequence sequence = (flags & 64) != 0 ? ReadSequence(input) : null;
				float rotation = (flags & 128) != 0 ? input.ReadFloat() : 0;
				float x = input.ReadFloat();
				float y = input.ReadFloat();
				float scaleX = input.ReadFloat();
				float scaleY = input.ReadFloat();
				float width = input.ReadFloat();
				float height = input.ReadFloat();

				if (path == null) path = name;
				RegionAttachment region = attachmentLoader.NewRegionAttachment(skin, name, path, sequence);
				if (region == null) return null;
				region.Path = path;
				region.x = x * scale;
				region.y = y * scale;
				region.scaleX = scaleX;
				region.scaleY = scaleY;
				region.rotation = rotation;
				region.width = width * scale;
				region.height = height * scale;
				region.r = ((color & 0xff000000) >> 24) / 255f;
				region.g = ((color & 0x00ff0000) >> 16) / 255f;
				region.b = ((color & 0x0000ff00) >> 8) / 255f;
				region.a = ((color & 0x000000ff)) / 255f;
				region.sequence = sequence;
				if (sequence == null) region.UpdateRegion();
				return region;
			}
			case AttachmentType.Boundingbox: {
				Vertices vertices = ReadVertices(input, (flags & 16) != 0);
				if (nonessential) input.ReadInt(); // discard, int color = nonessential ? input.readInt() : 0;

				BoundingBoxAttachment box = attachmentLoader.NewBoundingBoxAttachment(skin, name);
				if (box == null) return null;
				box.worldVerticesLength = vertices.length;
				box.vertices = vertices.vertices;
				box.bones = vertices.bones;
				// skipped porting: if (nonessential) Color.rgba8888ToColor(box.getColor(), color);
				return box;
			}
			case AttachmentType.Mesh: {
				string path = (flags & 16) != 0 ? input.ReadStringRef() : name;
				uint color = (flags & 32) != 0 ? (uint)input.ReadInt() : 0xffffffff;
				Sequence sequence = (flags & 64) != 0 ? ReadSequence(input) : null;
				int hullLength = input.ReadInt(true);
				Vertices vertices = ReadVertices(input, (flags & 128) != 0);
				float[] uvs = ReadFloatArray(input, vertices.length, 1);
				int[] triangles = ReadShortArray(input, (vertices.length - hullLength - 2) * 3);

				int[] edges = null;
				float width = 0, height = 0;
				if (nonessential) {
					edges = ReadShortArray(input, input.ReadInt(true));
					width = input.ReadFloat();
					height = input.ReadFloat();
				}

				MeshAttachment mesh = attachmentLoader.NewMeshAttachment(skin, name, path, sequence);
				if (mesh == null) return null;
				mesh.Path = path;
				mesh.r = ((color & 0xff000000) >> 24) / 255f;
				mesh.g = ((color & 0x00ff0000) >> 16) / 255f;
				mesh.b = ((color & 0x0000ff00) >> 8) / 255f;
				mesh.a = ((color & 0x000000ff)) / 255f;
				mesh.bones = vertices.bones;
				mesh.vertices = vertices.vertices;
				mesh.WorldVerticesLength = vertices.length;
				mesh.triangles = triangles;
				mesh.regionUVs = uvs;
				if (sequence == null) mesh.UpdateRegion();
				mesh.HullLength = hullLength << 1;
				mesh.Sequence = sequence;
				if (nonessential) {
					mesh.Edges = edges;
					mesh.Width = width * scale;
					mesh.Height = height * scale;
				}
				return mesh;
			}
			case AttachmentType.Linkedmesh: {
				String path = (flags & 16) != 0 ? input.ReadStringRef() : name;
				uint color = (flags & 32) != 0 ? (uint)input.ReadInt() : 0xffffffff;
				Sequence sequence = (flags & 64) != 0 ? ReadSequence(input) : null;
				bool inheritTimelines = (flags & 128) != 0;
				int skinIndex = input.ReadInt(true);
				string parent = input.ReadStringRef();
				float width = 0, height = 0;
				if (nonessential) {
					width = input.ReadFloat();
					height = input.ReadFloat();
				}

				MeshAttachment mesh = attachmentLoader.NewMeshAttachment(skin, name, path, sequence);
				if (mesh == null) return null;
				mesh.Path = path;
				mesh.r = ((color & 0xff000000) >> 24) / 255f;
				mesh.g = ((color & 0x00ff0000) >> 16) / 255f;
				mesh.b = ((color & 0x0000ff00) >> 8) / 255f;
				mesh.a = ((color & 0x000000ff)) / 255f;
				mesh.Sequence = sequence;
				if (nonessential) {
					mesh.Width = width * scale;
					mesh.Height = height * scale;
				}
				linkedMeshes.Add(new LinkedMesh(mesh, skinIndex, slotIndex, parent, inheritTimelines));
				return mesh;
			}
			case AttachmentType.Path: {
				bool closed = (flags & 16) != 0;
				bool constantSpeed = (flags & 32) != 0;
				Vertices vertices = ReadVertices(input, (flags & 64) != 0);
				float[] lengths = new float[vertices.length / 6];
				for (int i = 0, n = lengths.Length; i < n; i++)
					lengths[i] = input.ReadFloat() * scale;
				if (nonessential) input.ReadInt(); //int color = nonessential ? input.ReadInt() : 0;

				PathAttachment path = attachmentLoader.NewPathAttachment(skin, name);
				if (path == null) return null;
				path.closed = closed;
				path.constantSpeed = constantSpeed;
				path.worldVerticesLength = vertices.length;
				path.vertices = vertices.vertices;
				path.bones = vertices.bones;
				path.lengths = lengths;
				// skipped porting: if (nonessential) Color.rgba8888ToColor(path.getColor(), color);
				return path;
			}
			case AttachmentType.Point: {
				float rotation = input.ReadFloat();
				float x = input.ReadFloat();
				float y = input.ReadFloat();
				if (nonessential) input.ReadInt(); //int color = nonessential ? input.ReadInt() : 0;

				PointAttachment point = attachmentLoader.NewPointAttachment(skin, name);
				if (point == null) return null;
				point.x = x * scale;
				point.y = y * scale;
				point.rotation = rotation;
				// skipped porting: if (nonessential) point.color = color;
				return point;
			}
			case AttachmentType.Clipping: {
				int endSlotIndex = input.ReadInt(true);
				Vertices vertices = ReadVertices(input, (flags & 16) != 0);
				if (nonessential) input.ReadInt();

				ClippingAttachment clip = attachmentLoader.NewClippingAttachment(skin, name);
				if (clip == null) return null;
				clip.EndSlot = skeletonData.slots.Items[endSlotIndex];
				clip.worldVerticesLength = vertices.length;
				clip.vertices = vertices.vertices;
				clip.bones = vertices.bones;
				// skipped porting: if (nonessential) Color.rgba8888ToColor(clip.getColor(), color);
				return clip;
			}
			}
			return null;
		}

		private Sequence ReadSequence (SkeletonInput input) {
			Sequence sequence = new Sequence(input.ReadInt(true));
			sequence.Start = input.ReadInt(true);
			sequence.Digits = input.ReadInt(true);
			sequence.SetupIndex = input.ReadInt(true);
			return sequence;
		}

		private Vertices ReadVertices (SkeletonInput input, bool weighted) {
			float scale = this.scale;
			int vertexCount = input.ReadInt(true);
			Vertices vertices = new Vertices();
			vertices.length = vertexCount << 1;
			if (!weighted) {
				vertices.vertices = ReadFloatArray(input, vertices.length, scale);
				return vertices;
			}
			ExposedList<float> weights = new ExposedList<float>(vertices.length * 3 * 3);
			ExposedList<int> bonesArray = new ExposedList<int>(vertices.length * 3);
			for (int i = 0; i < vertexCount; i++) {
				int boneCount = input.ReadInt(true);
				bonesArray.Add(boneCount);
				for (int ii = 0; ii < boneCount; ii++) {
					bonesArray.Add(input.ReadInt(true));
					weights.Add(input.ReadFloat() * scale);
					weights.Add(input.ReadFloat() * scale);
					weights.Add(input.ReadFloat());
				}
			}

			vertices.vertices = weights.ToArray();
			vertices.bones = bonesArray.ToArray();
			return vertices;
		}

		private float[] ReadFloatArray (SkeletonInput input, int n, float scale) {
			float[] array = new float[n];
			if (scale == 1) {
				for (int i = 0; i < n; i++)
					array[i] = input.ReadFloat();
			} else {
				for (int i = 0; i < n; i++)
					array[i] = input.ReadFloat() * scale;
			}
			return array;
		}

		private int[] ReadShortArray (SkeletonInput input, int n) {
			int[] array = new int[n];
			for (int i = 0; i < n; i++)
				array[i] = input.ReadInt(true);
			return array;
		}

		/// <exception cref="SerializationException">SerializationException will be thrown when a Vertex attachment is not found.</exception>
		/// <exception cref="IOException">Throws IOException when a read operation fails.</exception>
		private Animation ReadAnimation (String name, SkeletonInput input, SkeletonData skeletonData) {
			ExposedList<Timeline> timelines = new ExposedList<Timeline>(input.ReadInt(true));
			float scale = this.scale;

			// Slot timelines.
			for (int i = 0, n = input.ReadInt(true); i < n; i++) {
				int slotIndex = input.ReadInt(true);
				for (int ii = 0, nn = input.ReadInt(true); ii < nn; ii++) {
					int timelineType = input.ReadUByte(), frameCount = input.ReadInt(true), frameLast = frameCount - 1;
					switch (timelineType) {
					case SLOT_ATTACHMENT: {
						AttachmentTimeline timeline = new AttachmentTimeline(frameCount, slotIndex);
						for (int frame = 0; frame < frameCount; frame++)
							timeline.SetFrame(frame, input.ReadFloat(), input.ReadStringRef());
						timelines.Add(timeline);
						break;
					}
					case SLOT_RGBA: {
						RGBATimeline timeline = new RGBATimeline(frameCount, input.ReadInt(true), slotIndex);
						float time = input.ReadFloat();
						float r = input.Read() / 255f, g = input.Read() / 255f;
						float b = input.Read() / 255f, a = input.Read() / 255f;
						for (int frame = 0, bezier = 0; ; frame++) {
							timeline.SetFrame(frame, time, r, g, b, a);
							if (frame == frameLast) break;
							float time2 = input.ReadFloat();
							float r2 = input.Read() / 255f, g2 = input.Read() / 255f;
							float b2 = input.Read() / 255f, a2 = input.Read() / 255f;
							switch (input.ReadUByte()) {
							case CURVE_STEPPED:
								timeline.SetStepped(frame);
								break;
							case CURVE_BEZIER:
								SetBezier(input, timeline, bezier++, frame, 0, time, time2, r, r2, 1);
								SetBezier(input, timeline, bezier++, frame, 1, time, time2, g, g2, 1);
								SetBezier(input, timeline, bezier++, frame, 2, time, time2, b, b2, 1);
								SetBezier(input, timeline, bezier++, frame, 3, time, time2, a, a2, 1);
								break;
							}
							time = time2;
							r = r2;
							g = g2;
							b = b2;
							a = a2;
						}
						timelines.Add(timeline);
						break;
					}
					case SLOT_RGB: {
						RGBTimeline timeline = new RGBTimeline(frameCount, input.ReadInt(true), slotIndex);
						float time = input.ReadFloat();
						float r = input.Read() / 255f, g = input.Read() / 255f, b = input.Read() / 255f;
						for (int frame = 0, bezier = 0; ; frame++) {
							timeline.SetFrame(frame, time, r, g, b);
							if (frame == frameLast) break;
							float time2 = input.ReadFloat();
							float r2 = input.Read() / 255f, g2 = input.Read() / 255f, b2 = input.Read() / 255f;
							switch (input.ReadUByte()) {
							case CURVE_STEPPED:
								timeline.SetStepped(frame);
								break;
							case CURVE_BEZIER:
								SetBezier(input, timeline, bezier++, frame, 0, time, time2, r, r2, 1);
								SetBezier(input, timeline, bezier++, frame, 1, time, time2, g, g2, 1);
								SetBezier(input, timeline, bezier++, frame, 2, time, time2, b, b2, 1);
								break;
							}
							time = time2;
							r = r2;
							g = g2;
							b = b2;
						}
						timelines.Add(timeline);
						break;
					}
					case SLOT_RGBA2: {
						RGBA2Timeline timeline = new RGBA2Timeline(frameCount, input.ReadInt(true), slotIndex);
						float time = input.ReadFloat();
						float r = input.Read() / 255f, g = input.Read() / 255f;
						float b = input.Read() / 255f, a = input.Read() / 255f;
						float r2 = input.Read() / 255f, g2 = input.Read() / 255f, b2 = input.Read() / 255f;
						for (int frame = 0, bezier = 0; ; frame++) {
							timeline.SetFrame(frame, time, r, g, b, a, r2, g2, b2);
							if (frame == frameLast) break;
							float time2 = input.ReadFloat();
							float nr = input.Read() / 255f, ng = input.Read() / 255f;
							float nb = input.Read() / 255f, na = input.Read() / 255f;
							float nr2 = input.Read() / 255f, ng2 = input.Read() / 255f, nb2 = input.Read() / 255f;
							switch (input.ReadUByte()) {
							case CURVE_STEPPED:
								timeline.SetStepped(frame);
								break;
							case CURVE_BEZIER:
								SetBezier(input, timeline, bezier++, frame, 0, time, time2, r, nr, 1);
								SetBezier(input, timeline, bezier++, frame, 1, time, time2, g, ng, 1);
								SetBezier(input, timeline, bezier++, frame, 2, time, time2, b, nb, 1);
								SetBezier(input, timeline, bezier++, frame, 3, time, time2, a, na, 1);
								SetBezier(input, timeline, bezier++, frame, 4, time, time2, r2, nr2, 1);
								SetBezier(input, timeline, bezier++, frame, 5, time, time2, g2, ng2, 1);
								SetBezier(input, timeline, bezier++, frame, 6, time, time2, b2, nb2, 1);
								break;
							}
							time = time2;
							r = nr;
							g = ng;
							b = nb;
							a = na;
							r2 = nr2;
							g2 = ng2;
							b2 = nb2;
						}
						timelines.Add(timeline);
						break;
					}
					case SLOT_RGB2: {
						RGB2Timeline timeline = new RGB2Timeline(frameCount, input.ReadInt(true), slotIndex);
						float time = input.ReadFloat();
						float r = input.Read() / 255f, g = input.Read() / 255f, b = input.Read() / 255f;
						float r2 = input.Read() / 255f, g2 = input.Read() / 255f, b2 = input.Read() / 255f;
						for (int frame = 0, bezier = 0; ; frame++) {
							timeline.SetFrame(frame, time, r, g, b, r2, g2, b2);
							if (frame == frameLast) break;
							float time2 = input.ReadFloat();
							float nr = input.Read() / 255f, ng = input.Read() / 255f, nb = input.Read() / 255f;
							float nr2 = input.Read() / 255f, ng2 = input.Read() / 255f, nb2 = input.Read() / 255f;
							switch (input.ReadUByte()) {
							case CURVE_STEPPED:
								timeline.SetStepped(frame);
								break;
							case CURVE_BEZIER:
								SetBezier(input, timeline, bezier++, frame, 0, time, time2, r, nr, 1);
								SetBezier(input, timeline, bezier++, frame, 1, time, time2, g, ng, 1);
								SetBezier(input, timeline, bezier++, frame, 2, time, time2, b, nb, 1);
								SetBezier(input, timeline, bezier++, frame, 3, time, time2, r2, nr2, 1);
								SetBezier(input, timeline, bezier++, frame, 4, time, time2, g2, ng2, 1);
								SetBezier(input, timeline, bezier++, frame, 5, time, time2, b2, nb2, 1);
								break;
							}
							time = time2;
							r = nr;
							g = ng;
							b = nb;
							r2 = nr2;
							g2 = ng2;
							b2 = nb2;
						}
						timelines.Add(timeline);
						break;
					}
					case SLOT_ALPHA: {
						AlphaTimeline timeline = new AlphaTimeline(frameCount, input.ReadInt(true), slotIndex);
						float time = input.ReadFloat(), a = input.Read() / 255f;
						for (int frame = 0, bezier = 0; ; frame++) {
							timeline.SetFrame(frame, time, a);
							if (frame == frameLast) break;
							float time2 = input.ReadFloat();
							float a2 = input.Read() / 255f;
							switch (input.ReadUByte()) {
							case CURVE_STEPPED:
								timeline.SetStepped(frame);
								break;
							case CURVE_BEZIER:
								SetBezier(input, timeline, bezier++, frame, 0, time, time2, a, a2, 1);
								break;
							}
							time = time2;
							a = a2;
						}
						timelines.Add(timeline);
						break;
					}
					}
				}
			}

			// Bone timelines.
			for (int i = 0, n = input.ReadInt(true); i < n; i++) {
				int boneIndex = input.ReadInt(true);
				for (int ii = 0, nn = input.ReadInt(true); ii < nn; ii++) {
					int type = input.ReadUByte(), frameCount = input.ReadInt(true);
					if (type == BONE_INHERIT) {
						InheritTimeline timeline = new InheritTimeline(frameCount, boneIndex);
						for (int frame = 0; frame < frameCount; frame++)
							timeline.SetFrame(frame, input.ReadFloat(), InheritEnum.Values[input.ReadUByte()]);
						timelines.Add(timeline);
						continue;
					}
					int bezierCount = input.ReadInt(true);
					switch (type) {
					case BONE_ROTATE:
						ReadTimeline(input, timelines, new RotateTimeline(frameCount, bezierCount, boneIndex), 1);
						break;
					case BONE_TRANSLATE:
						ReadTimeline(input, timelines, new TranslateTimeline(frameCount, bezierCount, boneIndex), scale);
						break;
					case BONE_TRANSLATEX:
						ReadTimeline(input, timelines, new TranslateXTimeline(frameCount, bezierCount, boneIndex), scale);
						break;
					case BONE_TRANSLATEY:
						ReadTimeline(input, timelines, new TranslateYTimeline(frameCount, bezierCount, boneIndex), scale);
						break;
					case BONE_SCALE:
						ReadTimeline(input, timelines, new ScaleTimeline(frameCount, bezierCount, boneIndex), 1);
						break;
					case BONE_SCALEX:
						ReadTimeline(input, timelines, new ScaleXTimeline(frameCount, bezierCount, boneIndex), 1);
						break;
					case BONE_SCALEY:
						ReadTimeline(input, timelines, new ScaleYTimeline(frameCount, bezierCount, boneIndex), 1);
						break;
					case BONE_SHEAR:
						ReadTimeline(input, timelines, new ShearTimeline(frameCount, bezierCount, boneIndex), 1);
						break;
					case BONE_SHEARX:
						ReadTimeline(input, timelines, new ShearXTimeline(frameCount, bezierCount, boneIndex), 1);
						break;
					case BONE_SHEARY:
						ReadTimeline(input, timelines, new ShearYTimeline(frameCount, bezierCount, boneIndex), 1);
						break;
					}
				}
			}

			// IK constraint timelines.
			for (int i = 0, n = input.ReadInt(true); i < n; i++) {
				int index = input.ReadInt(true), frameCount = input.ReadInt(true), frameLast = frameCount - 1;
				IkConstraintTimeline timeline = new IkConstraintTimeline(frameCount, input.ReadInt(true), index);
				int flags = input.Read();
				float time = input.ReadFloat(), mix = (flags & 1) != 0 ? ((flags & 2) != 0 ? input.ReadFloat() : 1) : 0;
				float softness = (flags & 4) != 0 ? input.ReadFloat() * scale : 0;
				for (int frame = 0, bezier = 0; ; frame++) {
					timeline.SetFrame(frame, time, mix, softness, (flags & 8) != 0 ? 1 : -1, (flags & 16) != 0, (flags & 32) != 0);

					if (frame == frameLast) break;
					flags = input.Read();
					float time2 = input.ReadFloat(), mix2 = (flags & 1) != 0 ? ((flags & 2) != 0 ? input.ReadFloat() : 1) : 0;
					float softness2 = (flags & 4) != 0 ? input.ReadFloat() * scale : 0;
					if ((flags & 64) != 0)
						timeline.SetStepped(frame);
					else if ((flags & 128) != 0) {
						SetBezier(input, timeline, bezier++, frame, 0, time, time2, mix, mix2, 1);
						SetBezier(input, timeline, bezier++, frame, 1, time, time2, softness, softness2, scale);
					}
					time = time2;
					mix = mix2;
					softness = softness2;
				}
				timelines.Add(timeline);
			}

			// Transform constraint timelines.
			for (int i = 0, n = input.ReadInt(true); i < n; i++) {
				int index = input.ReadInt(true), frameCount = input.ReadInt(true), frameLast = frameCount - 1;
				TransformConstraintTimeline timeline = new TransformConstraintTimeline(frameCount, input.ReadInt(true), index);
				float time = input.ReadFloat(), mixRotate = input.ReadFloat(), mixX = input.ReadFloat(), mixY = input.ReadFloat(),
				mixScaleX = input.ReadFloat(), mixScaleY = input.ReadFloat(), mixShearY = input.ReadFloat();
				for (int frame = 0, bezier = 0; ; frame++) {
					timeline.SetFrame(frame, time, mixRotate, mixX, mixY, mixScaleX, mixScaleY, mixShearY);
					if (frame == frameLast) break;
					float time2 = input.ReadFloat(), mixRotate2 = input.ReadFloat(), mixX2 = input.ReadFloat(), mixY2 = input.ReadFloat(),
					mixScaleX2 = input.ReadFloat(), mixScaleY2 = input.ReadFloat(), mixShearY2 = input.ReadFloat();
					switch (input.ReadUByte()) {
					case CURVE_STEPPED:
						timeline.SetStepped(frame);
						break;
					case CURVE_BEZIER:
						SetBezier(input, timeline, bezier++, frame, 0, time, time2, mixRotate, mixRotate2, 1);
						SetBezier(input, timeline, bezier++, frame, 1, time, time2, mixX, mixX2, 1);
						SetBezier(input, timeline, bezier++, frame, 2, time, time2, mixY, mixY2, 1);
						SetBezier(input, timeline, bezier++, frame, 3, time, time2, mixScaleX, mixScaleX2, 1);
						SetBezier(input, timeline, bezier++, frame, 4, time, time2, mixScaleY, mixScaleY2, 1);
						SetBezier(input, timeline, bezier++, frame, 5, time, time2, mixShearY, mixShearY2, 1);
						break;
					}
					time = time2;
					mixRotate = mixRotate2;
					mixX = mixX2;
					mixY = mixY2;
					mixScaleX = mixScaleX2;
					mixScaleY = mixScaleY2;
					mixShearY = mixShearY2;
				}
				timelines.Add(timeline);
			}

			// Path constraint timelines.
			for (int i = 0, n = input.ReadInt(true); i < n; i++) {
				int index = input.ReadInt(true);
				PathConstraintData data = skeletonData.pathConstraints.Items[index];
				for (int ii = 0, nn = input.ReadInt(true); ii < nn; ii++) {
					int type = input.ReadUByte(), frameCount = input.ReadInt(true), bezierCount = input.ReadInt(true);
					switch (type) {
					case PATH_POSITION:
						ReadTimeline(input, timelines, new PathConstraintPositionTimeline(frameCount, bezierCount, index),
							data.positionMode == PositionMode.Fixed ? scale : 1);
						break;
					case PATH_SPACING:
						ReadTimeline(input, timelines, new PathConstraintSpacingTimeline(frameCount, bezierCount, index),
							data.spacingMode == SpacingMode.Length || data.spacingMode == SpacingMode.Fixed ? scale : 1);
						break;
					case PATH_MIX:
						PathConstraintMixTimeline timeline = new PathConstraintMixTimeline(frameCount, bezierCount, index);
						float time = input.ReadFloat(), mixRotate = input.ReadFloat(), mixX = input.ReadFloat(), mixY = input.ReadFloat();
						for (int frame = 0, bezier = 0, frameLast = timeline.FrameCount - 1; ; frame++) {
							timeline.SetFrame(frame, time, mixRotate, mixX, mixY);
							if (frame == frameLast) break;
							float time2 = input.ReadFloat(), mixRotate2 = input.ReadFloat(), mixX2 = input.ReadFloat(),
								mixY2 = input.ReadFloat();
							switch (input.ReadUByte()) {
							case CURVE_STEPPED:
								timeline.SetStepped(frame);
								break;
							case CURVE_BEZIER:
								SetBezier(input, timeline, bezier++, frame, 0, time, time2, mixRotate, mixRotate2, 1);
								SetBezier(input, timeline, bezier++, frame, 1, time, time2, mixX, mixX2, 1);
								SetBezier(input, timeline, bezier++, frame, 2, time, time2, mixY, mixY2, 1);
								break;
							}
							time = time2;
							mixRotate = mixRotate2;
							mixX = mixX2;
							mixY = mixY2;
						}
						timelines.Add(timeline);
						break;
					}
				}
			}

			// Physics timelines.
			for (int i = 0, n = input.ReadInt(true); i < n; i++) {
				int index = input.ReadInt(true) - 1;
				for (int ii = 0, nn = input.ReadInt(true); ii < nn; ii++) {
					int type = input.ReadUByte(), frameCount = input.ReadInt(true);
					if (type == PHYSICS_RESET) {
						PhysicsConstraintResetTimeline timeline = new PhysicsConstraintResetTimeline(frameCount, index);
						for (int frame = 0; frame < frameCount; frame++)
							timeline.SetFrame(frame, input.ReadFloat());
						timelines.Add(timeline);
						continue;
					}
					int bezierCount = input.ReadInt(true);
					switch (type) {
					case PHYSICS_INERTIA:
						ReadTimeline(input, timelines, new PhysicsConstraintInertiaTimeline(frameCount, bezierCount, index), 1);
						break;
					case PHYSICS_STRENGTH:
						ReadTimeline(input, timelines, new PhysicsConstraintStrengthTimeline(frameCount, bezierCount, index), 1);
						break;
					case PHYSICS_DAMPING:
						ReadTimeline(input, timelines, new PhysicsConstraintDampingTimeline(frameCount, bezierCount, index), 1);
						break;
					case PHYSICS_MASS:
						ReadTimeline(input, timelines, new PhysicsConstraintMassTimeline(frameCount, bezierCount, index), 1);
						break;
					case PHYSICS_WIND:
						ReadTimeline(input, timelines, new PhysicsConstraintWindTimeline(frameCount, bezierCount, index), 1);
						break;
					case PHYSICS_GRAVITY:
						ReadTimeline(input, timelines, new PhysicsConstraintGravityTimeline(frameCount, bezierCount, index), 1);
						break;
					case PHYSICS_MIX:
						ReadTimeline(input, timelines, new PhysicsConstraintMixTimeline(frameCount, bezierCount, index), 1);
						break;
					}
				}
			}

			// Attachment timelines.
			for (int i = 0, n = input.ReadInt(true); i < n; i++) {
				Skin skin = skeletonData.skins.Items[input.ReadInt(true)];
				for (int ii = 0, nn = input.ReadInt(true); ii < nn; ii++) {
					int slotIndex = input.ReadInt(true);
					for (int iii = 0, nnn = input.ReadInt(true); iii < nnn; iii++) {
						String attachmentName = input.ReadStringRef();
						Attachment attachment = skin.GetAttachment(slotIndex, attachmentName);
						if (attachment == null) throw new SerializationException("Timeline attachment not found: " + attachmentName);

						int timelineType = input.ReadUByte(), frameCount = input.ReadInt(true), frameLast = frameCount - 1;
						switch (timelineType) {
						case ATTACHMENT_DEFORM: {
							VertexAttachment vertexAttachment = (VertexAttachment)attachment;
							bool weighted = vertexAttachment.Bones != null;
							float[] vertices = vertexAttachment.Vertices;
							int deformLength = weighted ? (vertices.Length / 3) << 1 : vertices.Length;

							DeformTimeline timeline = new DeformTimeline(frameCount, input.ReadInt(true), slotIndex, vertexAttachment);

							float time = input.ReadFloat();
							for (int frame = 0, bezier = 0; ; frame++) {
								float[] deform;
								int end = input.ReadInt(true);
								if (end == 0)
									deform = weighted ? new float[deformLength] : vertices;
								else {
									deform = new float[deformLength];
									int start = input.ReadInt(true);
									end += start;
									if (scale == 1) {
										for (int v = start; v < end; v++)
											deform[v] = input.ReadFloat();
									} else {
										for (int v = start; v < end; v++)
											deform[v] = input.ReadFloat() * scale;
									}
									if (!weighted) {
										for (int v = 0, vn = deform.Length; v < vn; v++)
											deform[v] += vertices[v];
									}
								}
								timeline.SetFrame(frame, time, deform);
								if (frame == frameLast) break;
								float time2 = input.ReadFloat();
								switch (input.ReadUByte()) {
								case CURVE_STEPPED:
									timeline.SetStepped(frame);
									break;
								case CURVE_BEZIER:
									SetBezier(input, timeline, bezier++, frame, 0, time, time2, 0, 1, 1);
									break;
								}
								time = time2;
							}
							timelines.Add(timeline);
							break;
						}
						case ATTACHMENT_SEQUENCE: {
							SequenceTimeline timeline = new SequenceTimeline(frameCount, slotIndex, attachment);
							for (int frame = 0; frame < frameCount; frame++) {
								float time = input.ReadFloat();
								int modeAndIndex = input.ReadInt();
								timeline.SetFrame(frame, time, (SequenceMode)(modeAndIndex & 0xf), modeAndIndex >> 4,
									input.ReadFloat());
							}
							timelines.Add(timeline);
							break;
						} // end case
						} // end switch
					}
				}
			}

			// Draw order timeline.
			int drawOrderCount = input.ReadInt(true);
			if (drawOrderCount > 0) {
				DrawOrderTimeline timeline = new DrawOrderTimeline(drawOrderCount);
				int slotCount = skeletonData.slots.Count;
				for (int i = 0; i < drawOrderCount; i++) {
					float time = input.ReadFloat();
					int offsetCount = input.ReadInt(true);
					int[] drawOrder = new int[slotCount];
					for (int ii = slotCount - 1; ii >= 0; ii--)
						drawOrder[ii] = -1;
					int[] unchanged = new int[slotCount - offsetCount];
					int originalIndex = 0, unchangedIndex = 0;
					for (int ii = 0; ii < offsetCount; ii++) {
						int slotIndex = input.ReadInt(true);
						// Collect unchanged items.
						while (originalIndex != slotIndex)
							unchanged[unchangedIndex++] = originalIndex++;
						// Set changed items.
						drawOrder[originalIndex + input.ReadInt(true)] = originalIndex++;
					}
					// Collect remaining unchanged items.
					while (originalIndex < slotCount)
						unchanged[unchangedIndex++] = originalIndex++;
					// Fill in unchanged items.
					for (int ii = slotCount - 1; ii >= 0; ii--)
						if (drawOrder[ii] == -1) drawOrder[ii] = unchanged[--unchangedIndex];
					timeline.SetFrame(i, time, drawOrder);
				}
				timelines.Add(timeline);
			}

			// Event timeline.
			int eventCount = input.ReadInt(true);
			if (eventCount > 0) {
				EventTimeline timeline = new EventTimeline(eventCount);
				for (int i = 0; i < eventCount; i++) {
					float time = input.ReadFloat();
					EventData eventData = skeletonData.events.Items[input.ReadInt(true)];
					Event e = new Event(time, eventData);
					e.intValue = input.ReadInt(false);
					e.floatValue = input.ReadFloat();
					e.stringValue = input.ReadString();
					if (e.stringValue == null) e.stringValue = eventData.String;
					if (e.Data.AudioPath != null) {
						e.volume = input.ReadFloat();
						e.balance = input.ReadFloat();
					}
					timeline.SetFrame(i, e);
				}
				timelines.Add(timeline);
			}

			float duration = 0;
			Timeline[] items = timelines.Items;
			for (int i = 0, n = timelines.Count; i < n; i++)
				duration = Math.Max(duration, items[i].Duration);
			return new Animation(name, timelines, duration);
		}

		/// <exception cref="IOException">Throws IOException when a read operation fails.</exception>
		private void ReadTimeline (SkeletonInput input, ExposedList<Timeline> timelines, CurveTimeline1 timeline, float scale) {
			float time = input.ReadFloat(), value = input.ReadFloat() * scale;
			for (int frame = 0, bezier = 0, frameLast = timeline.FrameCount - 1; ; frame++) {
				timeline.SetFrame(frame, time, value);
				if (frame == frameLast) break;
				float time2 = input.ReadFloat(), value2 = input.ReadFloat() * scale;
				switch (input.ReadUByte()) {
				case CURVE_STEPPED:
					timeline.SetStepped(frame);
					break;
				case CURVE_BEZIER:
					SetBezier(input, timeline, bezier++, frame, 0, time, time2, value, value2, scale);
					break;
				}
				time = time2;
				value = value2;
			}
			timelines.Add(timeline);
		}

		/// <exception cref="IOException">Throws IOException when a read operation fails.</exception>
		private void ReadTimeline (SkeletonInput input, ExposedList<Timeline> timelines, CurveTimeline2 timeline, float scale) {
			float time = input.ReadFloat(), value1 = input.ReadFloat() * scale, value2 = input.ReadFloat() * scale;
			for (int frame = 0, bezier = 0, frameLast = timeline.FrameCount - 1; ; frame++) {
				timeline.SetFrame(frame, time, value1, value2);
				if (frame == frameLast) break;
				float time2 = input.ReadFloat(), nvalue1 = input.ReadFloat() * scale, nvalue2 = input.ReadFloat() * scale;
				switch (input.ReadUByte()) {
				case CURVE_STEPPED:
					timeline.SetStepped(frame);
					break;
				case CURVE_BEZIER:
					SetBezier(input, timeline, bezier++, frame, 0, time, time2, value1, nvalue1, scale);
					SetBezier(input, timeline, bezier++, frame, 1, time, time2, value2, nvalue2, scale);
					break;
				}
				time = time2;
				value1 = nvalue1;
				value2 = nvalue2;
			}
			timelines.Add(timeline);
		}

		/// <exception cref="IOException">Throws IOException when a read operation fails.</exception>
		void SetBezier (SkeletonInput input, CurveTimeline timeline, int bezier, int frame, int value, float time1, float time2,
			float value1, float value2, float scale) {
			timeline.SetBezier(bezier, frame, value, time1, value1, input.ReadFloat(), input.ReadFloat() * scale, input.ReadFloat(),
					input.ReadFloat() * scale, time2, value2);
		}

		internal class Vertices {
			public int length;
			public int[] bones;
			public float[] vertices;
		}

		internal class SkeletonInput {
			private byte[] chars = new byte[32];
			private byte[] bytesBigEndian = new byte[8];
			internal string[] strings;
			Stream input;

			public SkeletonInput (Stream input) {
				this.input = input;
			}

			public int Read () {
				return input.ReadByte();
			}

			/// <summary>Explicit unsigned byte variant to prevent pitfalls porting Java reference implementation
			/// where byte is signed vs C# where byte is unsigned.</summary>
			public byte ReadUByte () {
				return (byte)input.ReadByte();
			}

			/// <summary>Explicit signed byte variant to prevent pitfalls porting Java reference implementation
			/// where byte is signed vs C# where byte is unsigned.</summary>
			public sbyte ReadSByte () {
				int value = input.ReadByte();
				if (value == -1) throw new EndOfStreamException();
				return (sbyte)value;
			}

			public bool ReadBoolean () {
				return input.ReadByte() != 0;
			}

			public float ReadFloat () {
				input.Read(bytesBigEndian, 0, 4);
				chars[3] = bytesBigEndian[0];
				chars[2] = bytesBigEndian[1];
				chars[1] = bytesBigEndian[2];
				chars[0] = bytesBigEndian[3];
				return BitConverter.ToSingle(chars, 0);
			}

			public int ReadInt () {
				input.Read(bytesBigEndian, 0, 4);
				return (bytesBigEndian[0] << 24)
					+ (bytesBigEndian[1] << 16)
					+ (bytesBigEndian[2] << 8)
					+ bytesBigEndian[3];
			}

			public long ReadLong () {
				input.Read(bytesBigEndian, 0, 8);
				return ((long)(bytesBigEndian[0]) << 56)
					+ ((long)(bytesBigEndian[1]) << 48)
					+ ((long)(bytesBigEndian[2]) << 40)
					+ ((long)(bytesBigEndian[3]) << 32)
					+ ((long)(bytesBigEndian[4]) << 24)
					+ ((long)(bytesBigEndian[5]) << 16)
					+ ((long)(bytesBigEndian[6]) << 8)
					+ (long)(bytesBigEndian[7]);
			}

			public int ReadInt (bool optimizePositive) {
				int b = input.ReadByte();
				int result = b & 0x7F;
				if ((b & 0x80) != 0) {
					b = input.ReadByte();
					result |= (b & 0x7F) << 7;
					if ((b & 0x80) != 0) {
						b = input.ReadByte();
						result |= (b & 0x7F) << 14;
						if ((b & 0x80) != 0) {
							b = input.ReadByte();
							result |= (b & 0x7F) << 21;
							if ((b & 0x80) != 0) result |= (input.ReadByte() & 0x7F) << 28;
						}
					}
				}
				return optimizePositive ? result : ((int)((uint)result >> 1) ^ -(result & 1));
			}

			public string ReadString () {
				int byteCount = ReadInt(true);
				switch (byteCount) {
				case 0:
					return null;
				case 1:
					return "";
				}
				byteCount--;
				byte[] buffer = this.chars;
				if (buffer.Length < byteCount) buffer = new byte[byteCount];
				ReadFully(buffer, 0, byteCount);
				return System.Text.Encoding.UTF8.GetString(buffer, 0, byteCount);
			}

			/// <return>May be null.</return>
			public String ReadStringRef () {
				int index = ReadInt(true);
				return index == 0 ? null : strings[index - 1];
			}

			public void ReadFully (byte[] buffer, int offset, int length) {
				while (length > 0) {
					int count = input.Read(buffer, offset, length);
					if (count <= 0) throw new EndOfStreamException();
					offset += count;
					length -= count;
				}
			}

			/// <summary>Returns the version string of binary skeleton data.</summary>
			public string GetVersionString () {
				try {
					// try reading 4.0+ format
					long initialPosition = input.Position;
					ReadLong(); // long hash

					long stringPosition = input.Position;
					int stringByteCount = ReadInt(true);
					input.Position = stringPosition;
					if (stringByteCount <= 13) {
						string version = ReadString();
						if (char.IsDigit(version[0]))
							return version;
					}
					// fallback to old version format
					input.Position = initialPosition;
					return GetVersionStringOld3X();
				} catch (Exception e) {
					throw new ArgumentException("Stream does not contain valid binary Skeleton Data.\n" + e, "input");
				}
			}

			/// <summary>Returns old 3.8 and earlier format version string of binary skeleton data.</summary>
			public string GetVersionStringOld3X () {
				// Hash.
				int byteCount = ReadInt(true);
				if (byteCount > 1) input.Position += byteCount - 1;

				// Version.
				byteCount = ReadInt(true);
				if (byteCount > 1 && byteCount <= 13) {
					byteCount--;
					byte[] buffer = new byte[byteCount];
					ReadFully(buffer, 0, byteCount);
					return System.Text.Encoding.UTF8.GetString(buffer, 0, byteCount);
				}
				throw new ArgumentException("Stream does not contain valid binary Skeleton Data.");
			}
		}

		private class LinkedMesh {
			internal string parent;
			internal int skinIndex, slotIndex;
			internal MeshAttachment mesh;
			internal bool inheritTimelines;

			public LinkedMesh (MeshAttachment mesh, int skinIndex, int slotIndex, string parent, bool inheritTimelines) {
				this.mesh = mesh;
				this.skinIndex = skinIndex;
				this.slotIndex = slotIndex;
				this.parent = parent;
				this.inheritTimelines = inheritTimelines;
			}
		}
	}
}
