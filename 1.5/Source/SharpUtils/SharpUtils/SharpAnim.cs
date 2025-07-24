using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace SharpUtils;

public class SharpAnim
{
	public class FrameStack : Graphic_Collection
	{
		public Graphic[] FrameArray()
		{
			return subGraphics;
		}

		public Graphic FrameAt(int i)
		{
			return subGraphics[i];
		}

		public Graphic RandomFrame()
		{
			return subGraphics.RandomElement();
		}

		public Material MatAt(int i)
		{
			return subGraphics[i].MatSingle;
		}

		public Material RandomMat()
		{
			return subGraphics.RandomElement().MatSingle;
		}

		public int FrameCount()
		{
			return subGraphics.Count();
		}
	}

	public class Frame : Graphic_Single
	{
		public Material FrameMat()
		{
			return mat;
		}
	}

	public class DrawOver
	{
		public Frame frame;

		public Thing thing;

		public float xSize;

		public float ySize;

		public float xOffset;

		public float yOffset;

		public DrawOver(Frame frame, Thing thing, float xSize, float ySize, float xOffset = 0f, float yOffset = 0f)
		{
			this.frame = frame;
			this.thing = thing;
			this.xSize = xSize;
			this.ySize = ySize;
			this.xOffset = xOffset;
			this.yOffset = yOffset;
		}

		public void Draw()
		{
			DrawFrame();
		}

		private void DrawFrame()
		{
			Matrix4x4 matrix = default(Matrix4x4);
			Vector3 vector = thing.TrueCenter();
			vector.x += xOffset;
			vector.z += yOffset;
			matrix.SetTRS(s: new Vector3(xSize, 1f, ySize), pos: vector + Altitudes.AltIncVect, q: thing.Rotation.AsQuat);
			Graphics.DrawMesh(MeshPool.plane10, matrix, frame.FrameMat(), 0);
		}
	}

	public class DrawOverMulti
	{
		public FrameStack frames;

		public Thing thing;

		private int CurrentFrame = 0;

		public float xSize;

		public float ySize;

		public float xOffset;

		public float yOffset;

		public DrawOverMulti(FrameStack frames, Thing thing, float xSize, float ySize, float xOffset = 0f, float yOffset = 0f)
		{
			this.frames = frames;
			this.thing = thing;
			this.xSize = xSize;
			this.ySize = ySize;
			this.xOffset = xOffset;
			this.yOffset = yOffset;
		}

		public void Draw()
		{
			DrawFrame(CurrentFrame);
		}

		public void NextFrame()
		{
			CurrentFrame++;
			if (CurrentFrame >= frames.FrameCount())
			{
				CurrentFrame = 0;
			}
		}

		public void PrevFrame()
		{
			CurrentFrame--;
			if (CurrentFrame < 0)
			{
				CurrentFrame = frames.FrameCount() - 1;
			}
		}

		public void SetFrame(int frame)
		{
			if (frame >= frames.FrameArray().Length || frame < 0)
			{
				Log.Error("[SharpUtils] Tried to set the frame of a DrawOverMulti to an invalid index!");
			}
			else
			{
				CurrentFrame = frame;
			}
		}

		private void DrawFrame(int FrameNum)
		{
			Matrix4x4 matrix = default(Matrix4x4);
			Vector3 vector = thing.TrueCenter();
			vector.x += xOffset;
			vector.z += yOffset;
			matrix.SetTRS(s: new Vector3(xSize, 1f, ySize), pos: vector + Altitudes.AltIncVect, q: thing.Rotation.AsQuat);
			Graphics.DrawMesh(MeshPool.plane10, matrix, frames.MatAt(FrameNum), 0);
		}
	}

	public class AnimateOver
	{
		public FrameStack frames;

		public Thing thing;

		private int CurrentFrame = 0;

		public int TicksPerFrame;

		private int TicksPrev;

		public float xSize;

		public float ySize;

		public float xOffset;

		public float yOffset;

		public AnimateOver(FrameStack frames, Thing thing, int TicksPerFrame, float xSize, float ySize, float xOffset = 0f, float yOffset = 0f)
		{
			this.frames = frames;
			this.thing = thing;
			this.TicksPerFrame = TicksPerFrame;
			this.xSize = xSize;
			this.ySize = ySize;
			this.xOffset = xOffset;
			this.yOffset = yOffset;
		}

		public void Draw()
		{
			int ticksGame = Find.TickManager.TicksGame;
			if (ticksGame >= TicksPrev + TicksPerFrame)
			{
				TicksPrev = ticksGame;
				CurrentFrame++;
			}
			if (CurrentFrame >= frames.FrameCount())
			{
				CurrentFrame = 0;
			}
			DrawFrame(CurrentFrame);
		}

		private void DrawFrame(int FrameNum)
		{
			Matrix4x4 matrix = default(Matrix4x4);
			Vector3 vector = thing.TrueCenter();
			vector.x += xOffset;
			vector.z += yOffset;
			matrix.SetTRS(s: new Vector3(xSize, 1f, ySize), pos: vector + Altitudes.AltIncVect, q: thing.Rotation.AsQuat);
			Graphics.DrawMesh(MeshPool.plane10, matrix, frames.MatAt(FrameNum), 0);
		}
	}

	public class SpinOver
	{
		public Frame frame;

		public Thing thing;

		public float xSize;

		public float ySize;

		public float xOffset;

		public float yOffset;

		private float CurRotationInt;

		public float DegreesPerTick;

		public int direction;

		public bool PowerDependent;

		private float CurRotation
		{
			get
			{
				return CurRotationInt;
			}
			set
			{
				if (!PowerDependent || thing.TryGetComp<CompPowerTrader>().PowerOn)
				{
					CurRotationInt = value;
					if (CurRotationInt > 360f)
					{
						CurRotationInt -= 360f;
					}
					if (CurRotationInt < 0f)
					{
						CurRotationInt += 360f;
					}
				}
			}
		}

		public SpinOver(Frame frame, Thing thing, float DegreesPerTick, float xSize, float ySize, float xOffset = 0f, float yOffset = 0f, int direction = 0, bool PowerDependent = false)
		{
			this.frame = frame;
			this.thing = thing;
			this.DegreesPerTick = DegreesPerTick;
			this.xSize = xSize;
			this.ySize = ySize;
			this.xOffset = xOffset;
			this.yOffset = yOffset;
			this.direction = direction;
			this.PowerDependent = PowerDependent;
		}

		public void Draw()
		{
			if (!Find.TickManager.Paused)
			{
				if (direction == 0)
				{
					CurRotation += DegreesPerTick * Find.TickManager.TickRateMultiplier;
				}
				else
				{
					CurRotation -= DegreesPerTick * Find.TickManager.TickRateMultiplier;
				}
			}
			DrawSpinner();
		}

		private void DrawSpinner()
		{
			Matrix4x4 matrix = default(Matrix4x4);
			Vector3 vector = thing.TrueCenter();
			vector.x += xOffset;
			vector.z += yOffset;
			matrix.SetTRS(s: new Vector3(xSize, 1f, ySize), pos: vector + Altitudes.AltIncVect, q: CurRotation.ToQuat());
			Graphics.DrawMesh(MeshPool.plane10, matrix, frame.FrameMat(), 0);
		}
	}

	public static FrameStack ConstructFrameStack(string path, Shader? shader = null, Vector2? DrawSize = null, Color? color = null)
	{
		if ((object)shader == null)
		{
			shader = ShaderDatabase.DefaultShader;
		}
		Vector2 valueOrDefault = DrawSize.GetValueOrDefault();
		if (!DrawSize.HasValue)
		{
			valueOrDefault = Vector2.one;
			DrawSize = valueOrDefault;
		}
		Color valueOrDefault2 = color.GetValueOrDefault();
		if (!color.HasValue)
		{
			valueOrDefault2 = Color.white;
			color = valueOrDefault2;
		}
		return (FrameStack)GraphicDatabase.Get<FrameStack>(path, shader, DrawSize.Value, color.Value);
	}

	public static Frame ConstructFrame(string path, Shader? shader = null, Vector2? DrawSize = null, Color? color = null)
	{
		if ((object)shader == null)
		{
			shader = ShaderDatabase.DefaultShader;
		}
		Vector2 valueOrDefault = DrawSize.GetValueOrDefault();
		if (!DrawSize.HasValue)
		{
			valueOrDefault = Vector2.one;
			DrawSize = valueOrDefault;
		}
		Color valueOrDefault2 = color.GetValueOrDefault();
		if (!color.HasValue)
		{
			valueOrDefault2 = Color.white;
			color = valueOrDefault2;
		}
		return (Frame)GraphicDatabase.Get<Frame>(path, shader, DrawSize.Value, color.Value);
	}
}
