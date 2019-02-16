#region Using Statements
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
//using Microsoft.Xna.Framework.GamerServices;
//using Microsoft.Xna.Framework.Input;
using WinFormsGraphicsDevice;
using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.ComponentModel;
using System.Runtime.InteropServices;

#endregion

namespace Visual_Music
{
	//using XnaKeys = Microsoft.Xna.Framework.Input.Keys;
	using WinKeys = System.Windows.Forms.Keys;
	using RectangleF = System.Drawing.RectangleF;
	using GdiPoint = System.Drawing.Point;

	public class SongPanel : GraphicsDeviceControl
	{
		public SpriteFont LyricsFont { get; private set; }
		GdiPoint previousMousePos = MousePosition;

		[DllImport("user32.dll")]
		static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

		//Jdlc.Timers.TimerQueueTimer timer = new Jdlc.Timers.TimerQueueTimer();
		System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
		public delegate void Delegate_songPosChanged();
		public Delegate_songPosChanged OnSongPosChanged { get; set; }
		public Project Project { get; set; }
		const int CmFaceSide = 4096;
		ScreenQuad quad;
		bool isRenderingVideo = false;
		ContentManager content;
		public ContentManager Content { get { return content; } }
		bool forceDefaultNoteStyle = false;
		public bool ForceDefaultNoteStyle
		{
			get => forceDefaultNoteStyle;
			set
			{
				forceDefaultNoteStyle = value;
				Invalidate();
			}
		}

		Texture2D regionSelectTexture;
		
		TimeSpan oldTime = new TimeSpan(0);
		Stopwatch stopwatch = new Stopwatch();
		public TimeSpan TotalTimeElapsed => stopwatch.Elapsed;
		double deltaTimeS;
		//double renderInterval = 0.0001;
		public bool LeftMbPressed { get; private set; } = false;
		public bool RightMbPressed { get; private set; } = false;
		double scrollCenter = 0;
		bool selectingRegion = false;
		public bool SelectingRegion => selectingRegion;
		bool mergeRegionSelection = false;
		bool mousePosScrollSong = false;
		bool isPausingWhileScrolling = false;
		
		//public void paint()
		//{
		//	OnPaint(null);
		//}

		Rectangle selectedScreenRegion;
		public float NormMouseX { get; set; }
		public float NormMouseY { get; set; }
		public SpriteBatch SpriteBatch { get; private set; }

		BlendState blendState;
		RasterizerState rastState;
		
		Point videoSize = new Point(1920, 1080);
		public readonly float SmallScrollStep = 1.0f / 16;
		public readonly float LargeScrollStep = 1.0f;

		public SongPanel()
		{
		}

		protected override void Initialize()
		{
			stopwatch.Start();
			SpriteBatch = new SpriteBatch(GraphicsDevice);
			blendState = BlendState.AlphaBlend;
			rastState = new RasterizerState()
			{
				MultiSampleAntiAlias = true,
				CullMode = CullMode.None
			};

			//BlendState.AlphaDestinationBlend = Blend.DestinationAlpha;
			//BlendState.AlphaSourceBlend = Blend.InverseDestinationAlpha;
			//BlendState.ColorDestinationBlend = Blend.DestinationAlpha;
			//BlendState.ColorSourceBlend = Blend.InverseDestinationAlpha;
			//BlendState.ColorWriteChannels = ColorWriteChannels.All;
			//BlendState.AlphaBlendFunction = BlendFunction.Add;
			//BlendState.ColorBlendFunction = BlendFunction.Add;
	
			//BlendState = BlendState.AlphaBlend;
			content = new ContentManager(Services, "Content");
			NoteStyle.sInitAllStyles(this);
			LyricsFont = Content.Load<SpriteFont>("Font");

			regionSelectTexture = new Texture2D(GraphicsDevice, 1, 1);
			regionSelectTexture.SetData(new[] { Color.White });

			quad = new ScreenQuad(this);

			timer.Interval = 1000 / 120;
			//timer.Elapsed += delegate { update(); };
			//timer.SynchronizingObject = this;
			timer.Tick += delegate { update(); };
			timer.Start();
		}

		public void update()
		{
			if (Project.Notes == null || isRenderingVideo)
				return;
			timer.Stop();
						
			//Thread.Sleep(100);

			TimeSpan newTime = stopwatch.Elapsed;
			deltaTimeS = (newTime - oldTime).TotalSeconds;
			//if (deltaTimeS < renderInterval)
			//return;

			oldTime = newTime;
			selectRegion();
			Project.update(deltaTimeS);
			scrollSong();
			
			if (Control.IsKeyLocked(WinKeys.CapsLock))
			{
				const int KEYEVENTF_EXTENDEDKEY = 0x1;
				const int KEYEVENTF_KEYUP = 0x2;
				keybd_event(0x14, 0x45, KEYEVENTF_EXTENDEDKEY, (UIntPtr)0);
				keybd_event(0x14, 0x45, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, (UIntPtr)0);
			}

			timer.Start();

		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
			}
			base.Dispose(disposing);
		}
		protected override void Draw()
		{
			GraphicsDevice.Clear(Color.Transparent);
			if (selectingRegion && selectedScreenRegion.Width != 0 && selectedScreenRegion.Height != 0)
			{
				SpriteBatch.Begin();
				Rectangle normRect = normalizeRect(selectedScreenRegion);
				SpriteBatch.Draw(regionSelectTexture, new Rectangle(normRect.Left, normRect.Top, normRect.Width, 1), Color.White);
				SpriteBatch.Draw(regionSelectTexture, new Rectangle(normRect.Left, normRect.Top, 1, normRect.Height), Color.White);
				SpriteBatch.Draw(regionSelectTexture, new Rectangle(normRect.Left, normRect.Bottom, normRect.Width, 1), Color.White);
				SpriteBatch.Draw(regionSelectTexture, new Rectangle(normRect.Right, normRect.Top, 1, normRect.Height), Color.White);
				SpriteBatch.End();
			}
			GraphicsDevice.BlendState = blendState;
			GraphicsDevice.RasterizerState = rastState;
			Project.drawSong();
		}

		void selectRegion()
		{
			if (Parent == null || ((Form1)Parent).trackListItems.Count == 0)
				return;
			
			if (LeftMbPressed)
			{
				Invalidate();
				selectingRegion = true;
				
				Point mousePos = new Point((int)((NormMouseX * 0.5f + 0.5f) * ClientRectangle.Width), (int)(NormMouseY * ClientRectangle.Height));
				selectedScreenRegion.Width = mousePos.X - selectedScreenRegion.X;
				selectedScreenRegion.Height = mousePos.Y - selectedScreenRegion.Y;

				int selectedCount = 0;

				//Frustum check----------------
				if (selectedScreenRegion.Width == 0 || selectedScreenRegion.Height == 0)
					return;

				//Normalize selection region so that the entire screen = [-1,-1] - [1,1]
				RectangleF normScreenSelection = new RectangleF((float)selectedScreenRegion.X / ClientRectangle.Width, (float)selectedScreenRegion.Y / ClientRectangle.Height, (float)selectedScreenRegion.Width / ClientRectangle.Width, (float)selectedScreenRegion.Height / ClientRectangle.Height);
				normScreenSelection.X *= 2;  normScreenSelection.Y *= -2;
				normScreenSelection.Width *= 2; normScreenSelection.Height *= -2;
				normScreenSelection.Offset(-1, 1);

				//Create frustum matrix
				Matrix selectionFrustumMat = Project.Camera.VpMat;

				//Scale frustum matrix so  that frustum shrinks to selection size
				float scaleX = 2 / Math.Abs(normScreenSelection.Width);
				float scaleY = 2.0f / Math.Abs(normScreenSelection.Height);
				selectionFrustumMat *= Matrix.CreateScale(scaleX, scaleY, 1);

				//Translate frustum matrix from screen center to selection center
				Vector2 normCenter = new Vector2(normScreenSelection.X + normScreenSelection.Width / 2, normScreenSelection.Y + normScreenSelection.Height / 2);
				selectionFrustumMat *= Matrix.CreateTranslation(-normCenter.X * scaleX, -normCenter.Y * scaleY, 0);

				//Create frustum
				BoundingFrustum selectionFrustum = new BoundingFrustum(selectionFrustumMat);

				float songPos = ((float)Project.SongPosT / Project.ViewWidthT + 0.5f) * Project.Camera.ViewportSize.X - Project.Camera.ViewportSize.X / 2.0f;
				for (int i = 1; i < Project.TrackViews.Count; i++)
				{
					if (Project.TrackViews[i].MidiTrack.Notes.Count > 0 &&
						Project.TrackViews[i].ocTree.areObjectsInFrustum(selectionFrustum, songPos, Project, Project.TrackViews[i].TrackProps))
					{
						((Form1)Parent).trackListItems[i].Selected = true;
						selectedCount++; ;
					}
					else if (!mergeRegionSelection && ((Form1)Parent).trackListItems.Count > 1)
						((Form1)Parent).trackListItems[i].Selected = false;
				}
				//----------------------

				if (selectedCount == 0 && !mergeRegionSelection)
					((Form1)Parent).trackListItems[0].Selected = true;
				else if (selectedCount > 0)
					((Form1)Parent).trackListItems[0].Selected = false;
			}
			else if (selectingRegion)
			{
				selectingRegion = false;
				Invalidate();
			}
		}

		Rectangle normalizeRect(Rectangle _rect)
		{
			Rectangle rect = new Rectangle(_rect.X, _rect.Y, _rect.Width, _rect.Height);
			if (rect.Height < 0)
			{
				int height = -rect.Height;
				rect.Y -= height;
				rect.Height = height;
			}
			if (rect.Width < 0)
			{
				int Width = -rect.Width;
				rect.X -= Width;
				rect.Width = Width;
			}
			return rect;
		}

		public void scrollSong()
		{
			if (mousePosScrollSong && !selectingRegion)
			{
				double dNormMouseX = (double)NormMouseX - scrollCenter;
				Project.NormSongPos += (float)(Math.Pow(dNormMouseX, 2) * Math.Sign(dNormMouseX) * deltaTimeS * 0.3f);
			}
		}

		public void renderVideo(string videoFilePath, RenderProgressForm progressForm, VideoExportOptions options)
		{
			lock (renderLock)
			{
				VideoFormat videoFormat = new VideoFormat((uint)options.Width, (uint)options.Height);
				//videoFormat.bitRate = 160000000;
				videoFormat.fps = options.Fps;
				videoFormat.aspectNumerator = 1;
				videoFormat.audioSampleRate = 44100;
				if (!Media.beginVideoEnc(videoFilePath, videoFormat, true))
				{
					lock (progressForm.cancelLock)
						progressForm.Cancel = true;
					progressForm.showMessage("Couldn't initialize video encoding.");
				}
				else
				{
					isRenderingVideo = true;
					RenderTarget2D[] renderTarget2d32bit = new RenderTarget2D[2];
					RenderTargetCube renderTargetCube = null;
					RenderTarget2D renderTarget2d8bit = null;
					RenderTarget2D renderTargetFinal = null;
					Effect cubeToPlaneFx = null;
					Effect ssFx = null;
					int frameSamples = 1;

					double normSongPosBackup = Project.NormSongPos;
					float viewWidthQnBackup = Project.ViewWidthQn;
					int maxPitchBackup = Project.MaxPitch;
					int minPitchBackup = Project.MinPitch;
					try
					{
						if (options.Sphere)
						{
							for (int i = 0; i < 2; i++)
								renderTarget2d32bit[i] = new RenderTarget2D(GraphicsDevice, options.SSAAWidth, options.SSAAHeight, options.EnableSSAA, SurfaceFormat.Vector4, DepthFormat.Depth24, 1, RenderTargetUsage.PreserveContents);
							renderTargetCube = new RenderTargetCube(GraphicsDevice, CmFaceSide, true, SurfaceFormat.Bgra32, DepthFormat.Depth24, 1, RenderTargetUsage.PreserveContents);
							cubeToPlaneFx = Content.Load<Effect>("CubeToPlane");
							cubeToPlaneFx.Parameters["CubeMap"].SetValue(renderTargetCube);
							cubeToPlaneFx.Parameters["FrameSamples"].SetValue((float)frameSamples);
						}

						renderTarget2d8bit = new RenderTarget2D(GraphicsDevice, options.SSAAWidth, options.SSAAHeight, options.EnableSSAA, SurfaceFormat.Bgra32, DepthFormat.Depth24, 1, RenderTargetUsage.PreserveContents);

						if (options.EnableSSAA)
							renderTargetFinal = new RenderTarget2D(GraphicsDevice, options.Width, options.Height, false, SurfaceFormat.Bgra32, DepthFormat.Depth24, 1, RenderTargetUsage.PreserveContents);
						else
							renderTargetFinal = renderTarget2d8bit;

						uint[] frameData = new uint[options.Width * options.Height];
						
						UInt64 frameDuration = 0;
						UInt64 frameStart = 0;
						int frames = 0;
						double songPosS = 0;
						
						if (options.Sphere)
						{
							//Project.ViewWidthQn /= 3;
							//int pitchChange = (int)((Project.MaxPitch - Project.MinPitch) / 5.0f);
							//Project.MaxPitch -= (int)(pitchChange / 1.3f);
							//Project.MinPitch += (int)(pitchChange * 1.3f); //Stretch downwards. It's easier for the neck to look down than up with vr glasses
							//Project.createOcTrees();
						}
						Project.Camera.InvertY = !options.Sphere;

						Project.setSongPosS(0, false);
						ssFx = Content.Load<Effect>("ss");
						while (Project.NormSongPos < 1 && !progressForm.Cancel)
						{
							Project.interpolateFrames();
							drawVideoFrame(songPosS, videoFormat.fps, frameSamples, options, renderTargetCube, renderTarget2d32bit, renderTarget2d8bit, cubeToPlaneFx);
							if (options.EnableSSAA)
							{
								GraphicsDevice.SetRenderTarget(renderTargetFinal);
								GraphicsDevice.Clear(Color.Transparent);
								ssFx.Parameters["FrameTex"].SetValue(renderTarget2d8bit);
								ssFx.CurrentTechnique.Passes[0].Apply();
								quad.draw();
							}
							GraphicsDevice.SetRenderTarget(null);
							renderTargetFinal.GetData<uint>(frameData);

							bool b = Media.writeFrame(frameData, frameStart, ref frameDuration, Project.AudioOffset + Project.PlaybackOffsetS);
							if (!b)
							{
								lock (progressForm.cancelLock)
									progressForm.Cancel = true;
								progressForm.showMessage("Couldn't add frame");
								break;
							}
							frameStart += frameDuration;
							double frameTimeS = 1.0 / videoFormat.fps;
							Project.setSongPosS(songPosS + frameTimeS, false);
							songPosS += frameTimeS;
							progressForm.updateProgress(Project.NormSongPos);
							frames++;
						}
					}
					catch (Exception e)
					{
						lock (progressForm.cancelLock)
							progressForm.Cancel = true;
						return;
					}
					finally
					{
						Project.NormSongPos = normSongPosBackup;
						Project.ViewWidthQn = viewWidthQnBackup;
						Project.MaxPitch = maxPitchBackup;
						Project.MinPitch = minPitchBackup;
						Project.createOcTrees();
						endVideoRender();
						
						renderTargetCube?.Dispose();
						for (int i = 0; i < 2; i++)
							renderTarget2d32bit[i]?.Dispose();
						renderTarget2d8bit?.Dispose();
						if (options.EnableSSAA)
							renderTargetFinal?.Dispose();
					}
					if (options.VrMetadata)
					{
						Process injector = new Process();
						injector.StartInfo.FileName = "minjector.exe";
						string stereo = options.Stereo ? "--stereo top-bottom" : "";
						injector.StartInfo.Arguments = " -i " + stereo + " \"" + videoFilePath + "\" \"" + videoFilePath + "_\"";
						injector.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
						injector.Start();
						injector.WaitForExit();
						File.Delete(videoFilePath);
						File.Move(videoFilePath + "_", videoFilePath);
					}
				}
			}
		}

		void drawVideoFrame(double songPosS, float fps, int frameSamples, VideoExportOptions options, RenderTargetCube renderTargetCube, RenderTarget2D[] renderTarget2d, RenderTarget2D renderTarget2d8bit, Effect cubeToPlaneFx)
		{
			RenderTarget2D rt = null;
			if (!options.Sphere)
				frameSamples = 1;
			for (int i = 0; i < frameSamples; i++)
			{
				//rt = renderTarget2d[i % 2];
				RenderTarget2D tex = null;
				if (i > 0)
					tex = renderTarget2d[(i + 1) % 2]; //Only blend with output of previous pass if not first pass
				if (i == frameSamples - 1)
					rt = renderTarget2d8bit; //Last pass should draw to normal rendertarget iwth 8 bits per channel
				drawVideoFrameSample(options, renderTargetCube, rt, tex, cubeToPlaneFx);

				double sampleTime = 1.0 / frameSamples / fps;
				Project.setSongPosS(songPosS + sampleTime, false);
				songPosS += sampleTime;
			}
		}

		delegate void DrawSceneToVideoFrameFunc();
		void drawVideoFrameSample(VideoExportOptions options, RenderTargetCube renderTargetCube, RenderTarget2D renderTarget2d, Texture2D prevFrame, Effect cubeToPlaneFx)
		{
			GraphicsDevice.SetRenderTarget(renderTarget2d);
			GraphicsDevice.Clear(Color.Transparent);
			if (options.Sphere)
			{
				if (options.Stereo)
				{
					drawSphere(renderTargetCube, renderTarget2d, prevFrame, cubeToPlaneFx, -1);
					drawSphere(renderTargetCube, renderTarget2d, prevFrame, cubeToPlaneFx, 1);
				}
				else
					drawSphere(renderTargetCube, renderTarget2d, prevFrame, cubeToPlaneFx);
			}
			else
			{
				Viewport viewport = GraphicsDevice.Viewport;
				if (options.Stereo)
				{
					Project.Camera.Eye = -1;
					GraphicsDevice.Viewport = new Viewport(0, 0, viewport.Width / 2, viewport.Height);

				}
				Project.drawSong();
				if (options.Stereo)
				{
					Project.Camera.Eye = 1;
					GraphicsDevice.Viewport = new Viewport(viewport.Width / 2, 0, viewport.Width / 2, viewport.Height);
					Project.drawSong();
				}
			}
		}

		void drawSphere(RenderTargetCube renderTargetCube, RenderTarget2D renderTarget2d, Texture2D prevFrame, Effect cubeToPlaneFx, int eye = 0)
		{
			Project.Camera.Eye = eye;


			for (int i = 0; i < 6; i++)
			{
				GraphicsDevice.SetRenderTarget(renderTargetCube, (CubeMapFace)Enum.ToObject(typeof(CubeMapFace), i));
				Project.Camera.CubeMapFace = i;
				GraphicsDevice.Clear(Color.Transparent);
				//GraphicsDevice.Clear(new Color((uint)i * 1000));
				Project.drawSong();
			}
			Project.Camera.Eye = 0;
			cubeToPlaneFx.Parameters["PrevFrame"].SetValue(prevFrame);
			cubeToPlaneFx.Parameters["IsFirstFrame"].SetValue(prevFrame == null);
			GraphicsDevice.SetRenderTarget(renderTarget2d);
			Viewport viewport = GraphicsDevice.Viewport;

			Vector4 vpBounds;
			Vector2 prevFrameSO;
			if (eye == 0)
			{
				vpBounds = new Vector4(0, 0, viewport.Width, viewport.Height);
				prevFrameSO = new Vector2(1, 0);
			}
			else if (eye == 1)
			{
				vpBounds = new Vector4(0, 0, viewport.Width, viewport.Height / 2);
				prevFrameSO = new Vector2(0.5f, -0.25f);
			}
			else if (eye == -1)
			{
				vpBounds = new Vector4(0, viewport.Height / 2, viewport.Width, viewport.Height / 2);
				prevFrameSO = new Vector2(0.5f, 0.25f);
			}
			else
				throw new Exception("Undefined eye index");

			cubeToPlaneFx.Parameters["ViewportSize"].SetValue(new Vector2(vpBounds.Z, vpBounds.W));
			cubeToPlaneFx.Parameters["PrevFrameScaleOffset"].SetValue(prevFrameSO);
			Vector3 lookAt = -Project.Camera.ViewMat.Right;
			Vector4 LookAt = new Vector4(lookAt.X, lookAt.Y, lookAt.Z, (float)Math.Cos(150f / 360 * Math.PI));
			cubeToPlaneFx.Parameters["LookAt"].SetValue(LookAt);
			GraphicsDevice.Viewport = new Viewport((int)vpBounds.X, (int)vpBounds.Y, (int)vpBounds.Z, (int)vpBounds.W);

			cubeToPlaneFx.CurrentTechnique.Passes[0].Apply();
			quad.draw();
		}

		void endVideoRender()
		{
			GraphicsDevice.SetRenderTarget(null);
			Media.endVideoEnc();
			Project.Camera.CubeMapFace = -1;
			Project.Camera.InvertY = false;
			Project.Camera.Eye = 0;
			quad.Pos = new Vector2(0, 0);
			quad.Size = new Vector2(1, 1);
			isRenderingVideo = false;
		}

		uint sampleCubeMap(Vector3 coords, uint[][] cmFaces, int faceSide)
		{
			int face;
			float absX = Math.Abs(coords.X);
			float absY = Math.Abs(coords.Y);
			float absZ = Math.Abs(coords.Z);

			bool isXPositive = coords.X > 0;
			bool isYPositive = coords.Y > 0;
			bool isZPositive = coords.Z > 0;

			float maxAxis, uc, vc;

			// POSITIVE X
			if (isXPositive && absX >= absY && absX >= absZ)
			{
				// u (0 to 1) goes from +z to -z
				// v (0 to 1) goes from -y to +y
				maxAxis = absX;
				uc = -coords.Z;
				vc = coords.Y;
				face = 0;
			}
			// NEGATIVE X
			else if (!isXPositive && absX >= absY && absX >= absZ)
			{
				// u (0 to 1) goes from -z to +z
				// v (0 to 1) goes from -y to +y
				maxAxis = absX;
				uc = coords.Z;
				vc = coords.Y;
				face = 1;
			}
			// POSITIVE Y
			else if (isYPositive && absY >= absX && absY >= absZ)
			{
				// u (0 to 1) goes from -x to +x
				// v (0 to 1) goes from +z to -z
				maxAxis = absY;
				uc = coords.X;
				vc = -coords.Z;
				face = 2;
			}
			// NEGATIVE Y
			else if (!isYPositive && absY >= absX && absY >= absZ)
			{
				// u (0 to 1) goes from -x to +x
				// v (0 to 1) goes from -z to +z
				maxAxis = absY;
				uc = coords.X;
				vc = coords.Z;
				face = 3;
			}
			// POSITIVE Z
			else if (isZPositive && absZ >= absX && absZ >= absY)
			{
				// u (0 to 1) goes from -x to +x
				// v (0 to 1) goes from -y to +y
				maxAxis = absZ;
				uc = coords.X;
				vc = coords.Y;
				face = 4;
			}
			// NEGATIVE Z
			else //if (!isZPositive && absZ >= absX && absZ >= absY)
			{
				// u (0 to 1) goes from +x to -x
				// v (0 to 1) goes from -y to +y
				maxAxis = absZ;
				uc = -coords.X;
				vc = coords.Y;
				face = 5;
			}

			// Convert range from -1 to 1 to 0 to cubemap size
			int u = (int)(0.5f * (uc / maxAxis + 1.0f) * (faceSide - 1));
			int v = (int)(0.5f * (vc / maxAxis + 1.0f) * (faceSide - 1));
			return cmFaces[face][v * faceSide + u];
		}

		public static Color HSLA2RGBA(Vector4 hsla)
		{
			float v;
			float r, g, b;
			float h = hsla.X, s = hsla.Y, l = hsla.Z;
			r = l;   // default to gray
			g = l;
			b = l;
			v = (l <= 0.5f) ? (l * (1.0f + s)) : (l + s - l * s);
			if (v > 0)
			{
				float m;
				float sv;
				int sextant;
				float fract, vsf, mid1, mid2;
				m = l + l - v;
				sv = (v - m) / v;
				h *= 6.0f;
				sextant = (int)h;
				fract = h - sextant;
				vsf = v * sv * fract;
				mid1 = m + vsf;
				mid2 = v - vsf;
				switch (sextant)
				{
					case 0:
						r = v;
						g = mid1;
						b = m;
						break;
					case 1:
						r = mid2;
						g = v;
						b = m;
						break;
					case 2:
						r = m;
						g = v;
						b = mid1;
						break;
					case 3:
						r = m;
						g = mid2;
						b = v;
						break;
					case 4:
						r = mid1;
						g = m;
						b = v;
						break;
					case 5:
						r = v;
						g = m;
						b = mid2;
						break;
				}
			}
			return new Color(r, g, b, hsla.W);
		}
		//NoteDrawingProps getNoteDrawingProps(int track, bool hilited)
		//{
		//    NoteDrawingProps props = new NoteDrawingProps();
		//    props.texture = trackProps[track].getTexture(hilited, GlobalTrackProps);
		//    props.color = trackProps[track].getColor(hilited, GlobalTrackProps, true);
		//    return props;
		//}

		public void updateTimeStamp()
		{
			oldTime = stopwatch.Elapsed;
		}
		
		protected override void OnResize(EventArgs e)
		{
			Invalidate();
			Project.createOcTrees();
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			base.OnMouseDown(e);
			Focus();
			if (Project.Notes == null)
				return;
			
			if (e.Button == MouseButtons.Left)
			{
				LeftMbPressed = true;
				if (ModifierKeys.HasFlag(WinKeys.Shift))
					mergeRegionSelection = true;
				selectedScreenRegion.X = (int)((NormMouseX * 0.5f + 0.5f) * ClientRectangle.Width);
				selectedScreenRegion.Y = (int)(NormMouseY * ClientRectangle.Height);
				
			}
			if (e.Button == MouseButtons.Right)
			{
				RightMbPressed = true;
				mousePosScrollSong = true;
				if (Project.IsPlaying)
				{
					isPausingWhileScrolling = true;
					Project.togglePlayback();
				}
			}

		}
		protected override void OnMouseUp(MouseEventArgs e)
		{
			base.OnMouseUp(e);
			if (e.Button == MouseButtons.Left)
			{
				LeftMbPressed = false;
				mergeRegionSelection = false;
			}
			if (e.Button == MouseButtons.Right)
			{
				RightMbPressed = false;
				mousePosScrollSong = false;
				if (isPausingWhileScrolling && !Project.IsPlaying)
				{
					Project.togglePlayback();
					isPausingWhileScrolling = false;
				}
			}
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);
			GdiPoint curPos = MousePosition;
			if (previousMousePos == MousePosition)
				return;
			previousMousePos = curPos;
			GdiPoint clientP = e.Location;
			int middleX = ClientRectangle.Width / 2;
			int middleY = ClientRectangle.Height / 2;

			NormMouseX = (float)(clientP.X - middleX) * 2 / ClientRectangle.Width;
			NormMouseY = (float)(clientP.Y) / ClientRectangle.Height;
			if (Project.Camera.MouseRot)
			{
				Invalidate();
				NormMouseY = (float)(clientP.Y - middleY) * 2 / ClientRectangle.Width;
				Cursor.Position = PointToScreen(new GdiPoint(middleX, middleY));
				Project.Camera.ApplyMouseRot(NormMouseX, NormMouseY);

			}
		}

		protected override void OnKeyDown(KeyEventArgs e)
		{
			base.OnKeyDown(e);
		
			//-----------------------------------

			//Project.Camera.toggleMouseControl(e.KeyCode, true))

			if (ModifierKeys != 0)
				return;

			//Control camera
			var keyFrame = Project.getKeyFrameAtSongPos();
			if (keyFrame == null)
				return;
			if (keyFrame.Camera.control(e.KeyCode, true))
			{
				Project.Camera.SpatialChanged?.Invoke();
				e.SuppressKeyPress = true;
			}

			//Start/stop playback
			if (e.KeyCode == WinKeys.Space)
			{
				Project.togglePlayback();
				e.SuppressKeyPress = true;
			}
			//Toggle force default note style
			if (e.KeyCode == WinKeys.Z)
			{
				ForceDefaultNoteStyle = true;
				for (int t = 1; t < Project.TrackViews.Count; t++)
				{
					TrackProps tprops = Project.TrackViews[t].TrackProps;
					NoteStyleType currentNoteStyle = (NoteStyleType)tprops.StyleProps.Type;
					if (tprops.ActiveNoteStyle.GetType() != typeof(NoteStyle_Bar))
					{
						tprops.StyleProps.Type = NoteStyleType.Bar;
						Project.TrackViews[t].createOcTree(Project, Project.GlobalTrackProps);
						tprops.StyleProps.Type = currentNoteStyle;
					}
				}
			}
		}

	}	
}

