using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace VisualMusic
{
    public enum LineType { Standard, Ribbon };
    public enum LineHlType { Arrow, Circle };

    public struct LineHlVertex : IVertexType
    {
        public Vector3 pos;
        public readonly static VertexDeclaration VertexDeclaration = new VertexDeclaration(new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0));
        VertexDeclaration IVertexType.VertexDeclaration
        {
            get { return VertexDeclaration; }
        }
    }

    public struct LinePatchVertex
    {
        public Vector3 center;
        public Vector4 texCoords;
        public Vector4 texCoords2;
        public Vector2 patchData;
    }

    [Serializable()]
    public class NoteStyle_Line : NoteStyle
    {
        const int MaxLinePatchVerts = 100000;
        const int ControlPointsPerPatch = 4;
        const int MaxPatchSubdivisionsPerBeat = 16;
        static VertexDeclaration s_lineVertDecl;
        static LinePatchVertex[] s_linePatchVerts = new LinePatchVertex[MaxLinePatchVerts];
        protected static LineHlVertex[] s_lineHlVerts = new LineHlVertex[4];

        float VpLineWidth => Project.NormalizeVpScalar((float)LineWidth);
        public float? LineWidth { get; set; } = 5;
        public float? Qn_gapThreshold { get; set; } = 3;
        public bool? Continuous { get; set; } = true;
        public LineType? LineType { get; set; } = VisualMusic.LineType.Standard;
        public LineHlType? HlType { get; set; } = LineHlType.Arrow;
        float VpHlSize => Project.NormalizeVpScalar((float)HlSize);
        public float? HlSize { get; set; } = 20;
        public bool? MovingHl { get; set; } = false;
        public bool? ShrinkingHl { get; set; } = false;
        public bool? HlBorder { get; set; } = false;
        public float? HlMovementPow { get; set; } = 1;

        public NoteStyle_Line()
        {
            _styleType = NoteStyleType.Line;
        }
        public NoteStyle_Line(TrackProps tprops)
            : base(tprops)
        {
            _styleType = NoteStyleType.Line;
        }
        public NoteStyle_Line(SerializationInfo info, StreamingContext ctxt)
            : base(info, ctxt)
        {
            foreach (var entry in info)
            {
                if (entry.Name == "lineWidth")
                    LineWidth = (float)entry.Value;
                else if (entry.Name == "qn_gapThreshold")
                    Qn_gapThreshold = (float)entry.Value;
                else if (entry.Name == "continuous")
                    Continuous = (bool)entry.Value;
                else if (entry.Name == "lineType")
                    LineType = (LineType)entry.Value;
                else if (entry.Name == "hlType")
                    HlType = (LineHlType)entry.Value;
                else if (entry.Name == "hlSize")
                    HlSize = (float)entry.Value;
                else if (entry.Name == "movingHl")
                    MovingHl = (bool)entry.Value;
                else if (entry.Name == "shrinkingHl")
                    ShrinkingHl = (bool)entry.Value;
                else if (entry.Name == "hlBorder")
                    HlBorder = (bool)entry.Value;
                else if (entry.Name == "hlMovementPow")
                    HlMovementPow = (float)entry.Value;
            }
        }
        override public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
        {
            base.GetObjectData(info, ctxt);
            info.AddValue("lineWidth", LineWidth);
            info.AddValue("qn_gapThreshold", Qn_gapThreshold);
            info.AddValue("continuous", Continuous);
            info.AddValue("LineType", LineType);
            info.AddValue("hlType", HlType);
            info.AddValue("hlSize", HlSize);
            info.AddValue("movingHl", MovingHl);
            info.AddValue("shrinkingHl", ShrinkingHl);
            info.AddValue("hlBorder", HlBorder);
            info.AddValue("hlMovementPow", HlMovementPow);
        }
        override public void LoadFx()
        {
            if (Content == null)
                throw new InvalidOperationException("NoteStyle.SetContent must run before LoadFx.");
            _fx = Content.Load<Effect>("Line");
        }

        readonly struct CubicBezier
        {
            public readonly Vector3 P0;
            public readonly Vector3 P1;
            public readonly Vector3 P2;
            public readonly Vector3 P3;

            public CubicBezier(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
            {
                P0 = p0;
                P1 = p1;
                P2 = p2;
                P3 = p3;
            }

            public Vector3 Evaluate(float t)
            {
                float s = 1 - t;
                return P0 * (s * s * s)
                    + P1 * (3 * s * s * t)
                    + P2 * (3 * s * t * t)
                    + P3 * (t * t * t);
            }

            public Vector3 Derivative(float t)
            {
                float s = 1 - t;
                return (P1 - P0) * (3 * s * s)
                    + (P2 - P1) * (6 * s * t)
                    + (P3 - P2) * (3 * t * t);
            }

            public CubicBezier Slice(float start, float end)
            {
                Vector3 startPos = Evaluate(start);
                Vector3 endPos = Evaluate(end);
                float range = end - start;
                return new CubicBezier(
                    startPos,
                    startPos + Derivative(start) * (range / 3),
                    endPos - Derivative(end) * (range / 3),
                    endPos);
            }

            public bool IsLinear
            {
                get
                {
                    Vector3 chord = P3 - P0;
                    float chordLength = chord.Length();
                    if (chordLength < 0.000001f)
                        return Vector3.DistanceSquared(P0, P1) < 0.0000001f
                            && Vector3.DistanceSquared(P0, P2) < 0.0000001f;
                    float distance1 = Math.Abs(chord.X * (P0.Y - P1.Y)
                        - (P0.X - P1.X) * chord.Y) / chordLength;
                    float distance2 = Math.Abs(chord.X * (P0.Y - P2.Y)
                        - (P0.X - P2.X) * chord.Y) / chordLength;
                    return Math.Max(distance1, distance2) < 0.000001f;
                }
            }

            public Vector3 GetControlPoint(int index) => index switch
            {
                0 => P0,
                1 => P1,
                2 => P2,
                3 => P3,
                _ => throw new ArgumentOutOfRangeException(nameof(index)),
            };
        }

        void CalcTexCoords(out Vector2 vert1Tc, out Vector2 vert2Tc, TrackPropsTex texProps, Texture2D texture, float stepFromNoteStart, float normStepFromNoteStart, float lineWidth, Vector3 worldPos1, Vector3 worldPos2, bool adjustingAspect = false)
        {
            double x1, y1, x2, y2;
            CalcTexCoords(out x1, out y1, out x2, out y2, texProps, texture, stepFromNoteStart, normStepFromNoteStart, lineWidth, worldPos1, worldPos2, false, false);
            vert1Tc = new Vector2((float)x1, (float)y1);
            vert2Tc = new Vector2((float)x2, (float)y2);
        }

        void CalcTexCoords(out double x1, out double y1, out double x2, out double y2, TrackPropsTex texProps, Texture2D texture, double stepFromNoteStart, double normStepFromNoteStart, double lineWidth, Vector3 worldPos1, Vector3 worldPos2, bool adjustingAspect, bool forceTiling)
        {
            double vpSizeX = Project.Props.Camera.ViewportSize.X;
            double vpSizeY = Project.Props.Camera.ViewportSize.Y;

            double texSizeX = (double)texture.Width * TexTileScale;
            double texSizeY = (double)texture.Height * TexTileScale;
            TexAnchorEnum texUAnchor = (TexAnchorEnum)texProps.UAnchor;
            TexAnchorEnum texVAnchor = (TexAnchorEnum)texProps.VAnchor;
            bool uTile = true, vTile = true;
            if (!forceTiling)
            {
                uTile = (bool)texProps.UTile;
                vTile = (bool)texProps.VTile;
            }

            //UAnchor
            if (texUAnchor == TexAnchorEnum.Note)
            {
                if (!uTile)
                    x1 = x2 = normStepFromNoteStart;
                else
                    x1 = x2 = stepFromNoteStart / texSizeX;
            }
            else if (texUAnchor == TexAnchorEnum.Screen)
            {
                if (!uTile)
                {
                    x1 = worldPos1.X / vpSizeX;
                    x2 = worldPos2.X / vpSizeX;
                }
                else
                {
                    x1 = worldPos1.X / texSizeX;
                    x2 = worldPos2.X / texSizeX;
                }
            }
            else if (texUAnchor == TexAnchorEnum.Song)
            {
                if (!uTile)
                {
                    x1 = (double)worldPos1.X / Project.SongLengthP;
                    x2 = (double)worldPos2.X / Project.SongLengthP;
                }
                else
                {
                    x1 = worldPos1.X / texSizeX;
                    x2 = worldPos2.X / texSizeX;
                }
            }
            else
                throw new NotImplementedException();

            //VAnchor
            if (texVAnchor == TexAnchorEnum.Note)
            {
                y1 = 0;
                if (!vTile)
                    y2 = 1;
                else
                    y2 = lineWidth / texSizeY;
            }
            else if (texVAnchor == TexAnchorEnum.Screen)
            {
                double worldPos1Y = worldPos1.Y + vpSizeY / 2;
                double worldPos2Y = worldPos2.Y + vpSizeY / 2;
                if (!vTile)
                {
                    y1 = worldPos1Y / vpSizeY;
                    y2 = worldPos2Y / vpSizeY;
                }
                else
                {
                    y1 = worldPos1Y / texSizeY;
                    y2 = worldPos2Y / texSizeY;
                }
            }
            else
                throw new NotImplementedException();
            if (!adjustingAspect)
                AdjustAspect(ref x1, ref y1, ref x2, ref y2, texProps, texture, stepFromNoteStart, normStepFromNoteStart, lineWidth, worldPos1, worldPos2);
            y1 *= -1;
            y2 *= -1;
        }

        void AdjustAspect(ref double x1, ref double y1, ref double x2, ref double y2, TrackPropsTex texProps, Texture2D texture, double stepFromNoteStart, double normStepFromNoteStart, double lineWidth, Vector3 worldPos1, Vector3 worldPos2)
        {
            if ((bool)texProps.KeepAspect)
            {
                double tiledX1, tiledY1, tiledX2, tiledY2;
                double regularX1 = x1, regularY1 = y1, regularX2 = x2, regularY2 = y2;
                float intWorldPosX = (int)worldPos1.X;
                worldPos1.X -= intWorldPosX;
                worldPos2.X -= intWorldPosX;
                CalcTexCoords(out regularX1, out regularY1, out regularX2, out regularY2, texProps, texture, stepFromNoteStart, normStepFromNoteStart, lineWidth, worldPos1, worldPos2, true, false);
                CalcTexCoords(out tiledX1, out tiledY1, out tiledX2, out tiledY2, texProps, texture, stepFromNoteStart, normStepFromNoteStart, lineWidth, worldPos1, worldPos2, true, true);
                double xDiff = regularX1 - regularX2, yDiff = regularY1 - regularY2;
                double tiledXDiff = tiledX1 - tiledX2, tiledYDiff = tiledY1 - tiledY2;

                if ((bool)texProps.UTile && !(bool)texProps.VTile)
                {
                    double ratio = yDiff / tiledYDiff;
                    x1 *= ratio;
                    x2 *= ratio;
                }
                else if (!(bool)texProps.UTile && (bool)texProps.VTile)
                {
                    double ratio;
                    if (tiledXDiff == 0)
                        ratio = x1 / tiledX1;
                    else
                        ratio = xDiff / tiledXDiff;
                    y1 *= ratio;
                    y2 *= ratio;
                }
            }
        }

        void SetHlCirclePos(Vector3 pos)
        {
            _fx.Parameters["WorldPos"].SetValue(pos);
            float halfHlSize = VpHlSize / 2.0f;
            s_lineHlVerts[0].pos = new Vector3(-halfHlSize, -halfHlSize, 0) + pos;
            s_lineHlVerts[1].pos = new Vector3(halfHlSize, -halfHlSize, 0) + pos;
            s_lineHlVerts[2].pos = new Vector3(-halfHlSize, halfHlSize, 0) + pos;
            s_lineHlVerts[3].pos = new Vector3(halfHlSize, halfHlSize, 0) + pos;
        }

        //static string GetName<T>(T item) where T : class
        //{
        //	return typeof(T).GetProperties()[0].Name;
        //}

        public override void CreateGeoChunk(out Geo geo, BoundingBox bbox, Midi.Track midiTrack, TrackProps trackProps, MaterialProps texMaterial)
        {
            LineGeo lineGeo = new LineGeo();
            geo = lineGeo;
            List<Midi.Note> noteList = midiTrack.Notes;
            if (noteList.Count == 0)
                return;

            int vertIndex = 0;
            float vpLineWidth = Math.Max(VpLineWidth, Project.NormalizeVpScalar(1f));
            float maxPatchTicks = Math.Max(1f,
                Project.Notes.TicksPerBeat / (float)MaxPatchSubdivisionsPerBeat);
            float boundsMargin = Project.NormalizeVpScalar(25f); // half of the UI's maximum line width
            Vector3 boundsMin = new Vector3(float.MaxValue, float.MaxValue, -0.001f);
            Vector3 boundsMax = new Vector3(float.MinValue, float.MinValue, 0.001f);
            float boundsStartTick = 0;
            bool hasBounds = false;

            void IncludeBounds(CubicBezier patch, float startTick, float endTick)
            {
                if (!hasBounds)
                {
                    boundsStartTick = startTick;
                    hasBounds = true;
                }

                for (int i = 0; i < ControlPointsPerPatch; i++)
                {
                    Vector3 point = patch.GetControlPoint(i);
                    boundsMin.X = Math.Min(boundsMin.X, point.X - boundsMargin);
                    boundsMin.Y = Math.Min(boundsMin.Y, point.Y - boundsMargin);
                    boundsMax.X = Math.Max(boundsMax.X, point.X + boundsMargin);
                    boundsMax.Y = Math.Max(boundsMax.Y, point.Y + boundsMargin);
                }

                if (endTick - boundsStartTick >= Project.Notes.TicksPerBeat * 4)
                    FlushBounds();
            }

            void FlushBounds()
            {
                if (!hasBounds)
                    return;
                lineGeo.bboxes.Add(new BoundingBoxEx(boundsMin, boundsMax));
                boundsMin = new Vector3(float.MaxValue, float.MaxValue, -0.001f);
                boundsMax = new Vector3(float.MinValue, float.MinValue, 0.001f);
                hasBounds = false;
            }

            for (int n = 0; n < noteList.Count; n++)
            {
                Midi.Note note = noteList[n];
                if (note.start > Project.Notes.SongLengthT)
                    continue;

                bool isLastNote = n == noteList.Count - 1;
                Midi.Note nextNote = isLastNote ? note : noteList[n + 1];
                float noteEndTick = isLastNote ? note.stop : Math.Min(note.stop, nextNote.start);
                bool endsAtNoteStop = isLastNote
                    || nextNote.start - note.stop > Qn_gapThreshold * Project.Notes.TicksPerBeat;
                float segmentEndTick = endsAtNoteStop ? noteEndTick : nextNote.start;
                if (segmentEndTick <= note.start)
                    continue;

                CubicBezier noteCurve = CreateNoteCurve(n, segmentEndTick, trackProps.TrackView.Curve);
                double rawPatchCount = noteCurve.IsLinear
                    ? 1
                    : Math.Ceiling((segmentEndTick - note.start) / maxPatchTicks);
                if (rawPatchCount > int.MaxValue)
                    throw new OverflowException($"Too many line patches for note {n}, track {trackProps.TrackView.TrackNumber}.");
                int patchCount = Math.Max(1, (int)rawPatchCount);
                float noteLength = noteCurve.P3.X - noteCurve.P0.X;

                for (int patchIndex = 0; patchIndex < patchCount; patchIndex++)
                {
                    if (vertIndex > MaxLinePatchVerts - ControlPointsPerPatch)
                        CreateLinePatchBuffer(ref vertIndex, lineGeo);

                    float patchStart = patchIndex / (float)patchCount;
                    float patchEnd = (patchIndex + 1) / (float)patchCount;
                    CubicBezier patch = noteCurve.Slice(patchStart, patchEnd);
                    for (int controlPoint = 0; controlPoint < ControlPointsPerPatch; controlPoint++)
                    {
                        float sampleT = controlPoint / 3f;
                        float noteT = MathHelper.Lerp(patchStart, patchEnd, sampleT);
                        s_linePatchVerts[vertIndex++] = CreatePatchVertex(
                            patch, controlPoint, sampleT, noteT, noteLength, vpLineWidth, texMaterial);
                    }
                    IncludeBounds(patch,
                        MathHelper.Lerp(note.start, segmentEndTick, patchStart),
                        MathHelper.Lerp(note.start, segmentEndTick, patchEnd));
                }
            }
            CreateLinePatchBuffer(ref vertIndex, lineGeo);
            FlushBounds();
        }

        CubicBezier CreateNoteCurve(int noteIndex, float endTick, Curve curve)
        {
            CurveKey startKey = curve.Keys[noteIndex];
            Vector3 p0 = new Vector3(
                Project.GetScreenPosX(startKey.Position),
                Project.GetScreenPosY(startKey.Value), 0);

            if (noteIndex < curve.Keys.Count - 1)
            {
                CurveKey endKey = curve.Keys[noteIndex + 1];
                float duration = endKey.Position - startKey.Position;
                if (duration > 0)
                {
                    Vector3 p3 = new Vector3(
                        Project.GetScreenPosX(endKey.Position),
                        Project.GetScreenPosY(endKey.Value), 0);
                    float dx = (p3.X - p0.X) / 3f;
                    CubicBezier fullCurve = new CubicBezier(
                        p0,
                        new Vector3(p0.X + dx,
                            p0.Y + startKey.TangentOut * Project.Props.NoteHeight / 3f, 0),
                        new Vector3(p3.X - dx,
                            p3.Y - endKey.TangentIn * Project.Props.NoteHeight / 3f, 0),
                        p3);
                    float end = MathHelper.Clamp((endTick - startKey.Position) / duration, 0, 1);
                    return fullCurve.Slice(0, end);
                }
            }

            Vector3 lineEnd = new Vector3(Project.GetScreenPosX(endTick), p0.Y, 0);
            Vector3 delta = (lineEnd - p0) / 3f;
            return new CubicBezier(p0, p0 + delta, p0 + delta * 2, lineEnd);
        }

        LinePatchVertex CreatePatchVertex(CubicBezier patch, int controlPoint, float sampleT,
            float noteT, float noteLength, float lineWidth, MaterialProps texMaterial)
        {
            Vector3 sampleCenter = patch.Evaluate(sampleT);
            Vector3 tangent = patch.Derivative(sampleT);
            if (tangent.LengthSquared() < 0.0000001f)
                tangent = Vector3.UnitX;
            Vector3 normal = new Vector3(-tangent.Y, tangent.X, 0);
            normal.Normalize();
            Vector3 offset = LineType == VisualMusic.LineType.Ribbon ? Vector3.UnitX : normal;
            offset *= lineWidth / 2;
            Vector3 side0 = sampleCenter - offset;
            Vector3 side1 = sampleCenter + offset;

            Vector2 tex0 = Vector2.Zero;
            Vector2 tex1 = Vector2.Zero;
            Vector2 tex20 = Vector2.Zero;
            Vector2 tex21 = Vector2.Zero;
            Texture2D texture = texMaterial.TexProps.Texture ?? texMaterial.TexProps.TransitionTexture;
            Texture2D texture2 = texMaterial.TexProps.TransitionTexture;
            if (texture != null)
            {
                CalcTexCoords(out tex0, out tex1, texMaterial.TexProps, texture,
                    noteT * noteLength, noteT, lineWidth, side0, side1);
                if (texture2 != null)
                    CalcTexCoords(out tex20, out tex21, texMaterial.TexProps, texture2,
                        noteT * noteLength, noteT, lineWidth, side0, side1);
                else
                {
                    tex20 = tex0;
                    tex21 = tex1;
                }
            }

            return new LinePatchVertex
            {
                center = patch.GetControlPoint(controlPoint),
                texCoords = new Vector4(tex0.X, tex0.Y, tex1.X, tex1.Y),
                texCoords2 = new Vector4(tex20.X, tex20.Y, tex21.X, tex21.Y),
                patchData = new Vector2(noteT, noteLength),
            };
        }

        public override void DrawGeoChunk(Geo geo)
        {
            LineGeo lineGeo = (LineGeo)geo;
            foreach (var vb in lineGeo.lineVb)
            {
                GraphicsDevice.SetVertexBuffer(vb);
                GraphicsDevice.DrawPrimitives(PrimitiveType.PatchListWith4ControlPoints,
                    0, vb.VertexCount / ControlPointsPerPatch);
            }
        }

        void CreateLinePatchBuffer(ref int numVerts, LineGeo geo)
        {
            if (numVerts == 0)
                return;
            VertexBuffer vb = new VertexBuffer(GraphicsDevice, s_lineVertDecl, numVerts, BufferUsage.WriteOnly);
            vb.SetData(s_linePatchVerts, 0, numVerts);
            geo.lineVb.Add(vb);
            numVerts = 0;
        }

        public override void DrawTrack(Midi.Track midiTrack, TrackProps trackProps, MaterialProps texMaterial)
        {
            float songPosP;

            base.DrawTrack(midiTrack, trackProps, texMaterial, out songPosP);
            List<Midi.Note> noteList = midiTrack.Notes;
            //List<Midi.Note> noteList = getNotes(0, midiTrack, songDrawProps);
            if (noteList.Count == 0)
                return;

            //this.trackProps = trackProps;

            _fx.Parameters["LineType"].SetValue((int)LineType);
            _fx.Parameters["HlSize"].SetValue(VpHlSize / 2.0f);
            _fx.Parameters["TessellationViewportSize"].SetValue(
                new Vector2(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height));
            float radius = (float)VpLineWidth / 2.0f;
            _fx.Parameters["Radius"].SetValue(radius);
            _fx.Parameters["InnerHlSize"].SetValue(0.0f);

            // Keep the buffers resident at zero width so a later shader-only width change can
            // reveal them immediately.  Skipping the draw also avoids Radius == 0 in the pixel
            // shader's normalized edge-distance calculation.
            if (radius > 0)
            {
                _fx.CurrentTechnique = _fx.Techniques["Line"];
                _fx.Parameters["DiscardAtOnce"].SetValue(true);
                _fx.CurrentTechnique.Passes[0].Apply();
                DrawGeoChunk(trackProps.TrackView.Geo);
                _fx.Parameters["DiscardAtOnce"].SetValue(false);
                _fx.CurrentTechnique.Passes[0].Apply();
                DrawGeoChunk(trackProps.TrackView.Geo);
            }

            DepthStencilState oldDss = GraphicsDevice.DepthStencilState;
            GraphicsDevice.DepthStencilState = DepthStencilState.None;
            DrawHighLights(midiTrack, trackProps, songPosP);
            GraphicsDevice.DepthStencilState = oldDss;
        }

        void DrawHighLights(Midi.Track midiTrack, TrackProps trackProps, float songPosP)
        {
            List<Midi.Note> noteList = midiTrack.Notes;
            double playbackTick = Project.SongPosT - Project.PlaybackOffsetT;
            int hlNoteIndex = GetHighlightedNoteIndex(midiTrack, playbackTick);
            if (hlNoteIndex < 0)
                return;
            Midi.Note note = noteList[hlNoteIndex], nextNote;
            if (note.start > Project.Notes.SongLengthT) //only  if audio ends before the notes end
                return;

            if (hlNoteIndex < noteList.Count - 1)
                nextNote = noteList[hlNoteIndex + 1];
            else
                nextNote = note;

            Vector3 noteStart = new Vector3(Project.GetScreenPos(note.start, note.pitch, trackProps), 0);
            float noteEnd = Project.GetScreenPos(note.stop, note.pitch, trackProps).X;

            Vector3 nextNoteStart = new Vector3(Project.GetScreenPos(nextNote.start, nextNote.pitch, trackProps), 0);
            if (noteEnd > nextNoteStart.X && hlNoteIndex < noteList.Count - 1)
                noteEnd = nextNoteStart.X;

            if ((float)(nextNote.start - note.stop) > Qn_gapThreshold * Project.Notes.TicksPerBeat || note == nextNote)
            {
                if (nextNoteStart.X != noteStart.X)
                    nextNoteStart.Y = MathHelper.Lerp(noteStart.Y, nextNoteStart.Y, (float)(noteEnd - noteStart.X) / (nextNoteStart.X - noteStart.X));
                nextNoteStart.X = noteEnd;
            }

            Vector3 hlPos = noteStart;
            float noteLength = (noteEnd - noteStart.X);
            float normPos = (songPosP - noteStart.X) / noteLength;
            if ((bool)MovingHl)
            {
                float poweredNormPos = (float)Math.Pow(normPos, (double)HlMovementPow);
                hlPos.X = noteStart.X + poweredNormPos * noteLength;
                hlPos.Y = Project.GetScreenPosY(trackProps.TrackView.Curve.Evaluate((float)Project.PixelsToTicks(hlPos.X, trackProps)));
            }

            //Set common fx params---------------------

            //For shrinking highlights
            float shrinkPercent = normPos * 1.0001f;
            if (!(bool)ShrinkingHl)
            {
                shrinkPercent = 0;
                if ((bool)HlBorder)
                    shrinkPercent = 1;
            }
            _fx.Parameters["ClipPercent"].SetValue(shrinkPercent);
            float innerHlSize = VpHlSize * 0.5f * (1 - shrinkPercent);
            _fx.Parameters["InnerHlSize"].SetValue(innerHlSize);

            //Vector4 hlColor;
            //Texture2D hlTexture;
            //getMaterial(trackProps, true, out hlColor, out hlTexture);
            _fx.Parameters["Border"].SetValue((bool)HlBorder);
            //-----------------------------------------------

            if (HlType == LineHlType.Arrow)
            {
                float arrowLength;
                Vector3 arrowDir;
                Vector3 arrowNormal;
                if (!(bool)MovingHl)  //Non-moving arrow
                {
                    Vector3 nextNoteOffset = nextNoteStart - noteStart;
                    arrowLength = nextNoteOffset.Length();
                    arrowDir = nextNoteOffset; ;
                }
                else //Moving arrow
                {
                    float x1 = hlPos.X;
                    float y1 = hlPos.Y;
                    float x2 = x1 + 0.001f;
                    float pitch2 = trackProps.TrackView.Curve.Evaluate((float)Project.PixelsToTicks(x2, trackProps));
                    float y2 = Project.GetScreenPosY(pitch2);
                    arrowDir = new Vector3(x2 - x1, y2 - y1, 0);
                    arrowLength = VpHlSize * 1.25f;  //Make arrow 25% longer than wide
                }
                arrowDir.Normalize();
                arrowNormal = new Vector3(-arrowDir.Y, arrowDir.X, 0);
                arrowNormal.Normalize();

                float halfArrowWidth = VpHlSize * 0.5f;

                s_lineHlVerts[0].pos = hlPos + arrowNormal * halfArrowWidth;
                s_lineHlVerts[1].pos = hlPos - arrowNormal * halfArrowWidth;
                s_lineHlVerts[2].pos = hlPos + arrowDir * arrowLength;

                _fx.Parameters["ArrowDir"].SetValue(arrowDir);
                //lineFx.Parameters["ArrowLength"].SetValue(nextNoteOffsetLength);
                _fx.Parameters["ArrowStart"].SetValue(hlPos);
                _fx.Parameters["ArrowEnd"].SetValue(s_lineHlVerts[2].pos); //Is used to calc distance from the two "sides" of the triangle (not the bottom) since they share this point
                Vector3 side1Tangent = s_lineHlVerts[2].pos - s_lineHlVerts[0].pos;
                Vector3 side1Normal = new Vector3(-side1Tangent.Y, side1Tangent.X, 0);
                side1Normal.Normalize();
                _fx.Parameters["Side1Normal"].SetValue(side1Normal);
                Vector3 side2Tangent = s_lineHlVerts[2].pos - s_lineHlVerts[1].pos;
                Vector3 side2Normal = new Vector3(-side2Tangent.Y, side2Tangent.X, 0);
                side2Normal.Normalize();
                _fx.Parameters["Side2Normal"].SetValue(side2Normal);

                //Calc shortest dist to incenter from border, ie. the inscribed circle's radius
                float a = (s_lineHlVerts[0].pos - s_lineHlVerts[1].pos).Length();
                float b = (s_lineHlVerts[0].pos - s_lineHlVerts[2].pos).Length();
                float c = (s_lineHlVerts[1].pos - s_lineHlVerts[2].pos).Length();
                float k = (a + b + c) / 2.0f;
                float icRadius = (float)Math.Sqrt(k * (k - a) * (k - b) * (k - c)) / k;
                _fx.Parameters["DistToCenter"].SetValue(icRadius);

                _fx.CurrentTechnique = _fx.Techniques["Arrow"];
                _fx.CurrentTechnique.Passes[0].Apply();
                GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, s_lineHlVerts, 0, 1);
            }
            else if (HlType == LineHlType.Circle)
            {
                SetHlCirclePos(hlPos);

                _fx.CurrentTechnique = _fx.Techniques["Circle"];
                _fx.CurrentTechnique.Passes[0].Apply();
                GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, s_lineHlVerts, 0, 2);
            }
        }

        internal static int GetHighlightedNoteIndex(Midi.Track midiTrack, double playbackTick)
        {
            int noteIndex = midiTrack.GetLastStartedNoteIndexAtTime((int)playbackTick);
            if (noteIndex < 0)
                return -1;

            Midi.Note note = midiTrack.Notes[noteIndex];
            return playbackTick >= note.start && playbackTick < note.stop ? noteIndex : -1;
        }

        float CalcLerpFactor(int x, int x1, int x2)
        {
            float f = (float)(x - x1) / (float)(x2 - x1);
            return (1 - (float)Math.Cos((double)f * Math.PI)) * 0.5f;
        }

        public static void SInit()
        {
            s_lineVertDecl = new VertexDeclaration(
                new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
                new VertexElement(12, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 0),
                new VertexElement(28, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 1),
                new VertexElement(44, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 2));
        }
    }

    public class LineGeo : Geo
    {
        public List<VertexBuffer> lineVb = new List<VertexBuffer>();
        protected override void ReleaseResources()
        {
            foreach (var vb in lineVb)
                vb.Dispose();
        }
    }
}

