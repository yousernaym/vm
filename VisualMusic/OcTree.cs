using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace VisualMusic
{
    public class OcTree<Geo> where Geo : VisualMusic.Geo
    {
        int refCOunt = 0;
        protected bool _isEmpty = true;
        protected BoundingBox _bbox;
        protected List<BoundingBox> _objects = new List<BoundingBox>();
        public Geo _geo;
        protected Vector3 _minSize;

        public delegate void CreateGeoChunk(out Geo geo, BoundingBox bbox, Midi.Track midiTrack, TrackProps trackProps, MaterialProps texMaterial);
        public delegate void DrawGeoChunk(Geo geo);
        CreateGeoChunk _createGeoChunk;
        DrawGeoChunk _drawGeoChunk;

        public OcTree(Vector3 minPos, Vector3 size, Vector3 minSize, CreateGeoChunk createGeoChunk, DrawGeoChunk drawGeoChunk)
        {
            _minSize = minSize;
            _bbox = new BoundingBox(minPos, minPos + size);
            _createGeoChunk = createGeoChunk;
            _drawGeoChunk = drawGeoChunk;
            if (size.X <= minSize.X && size.Y <= minSize.Y && size.Z <= minSize.Z)
                return;
        }

        public void createGeo(Midi.Track midiTrack, TrackProps trackProps, TrackProps globalTrackProps, MaterialProps texMaterial)
        {
            _createGeoChunk(out _geo, _bbox, midiTrack, trackProps, texMaterial);
        }

        public void drawGeo(Camera cam)
        {
            _drawGeoChunk(_geo);
        }

        public OcTree<Geo> AddRef()
        {
            refCOunt++;
            return this;
        }

        public void Dispose()
        {
            if (refCOunt-- == 0)
                throw new AccessViolationException("Object already disposed");
            if (refCOunt > 0)
                return;
            if (_geo != null)
                _geo.Dispose();
        }

        public bool areObjectsInFrustum(BoundingFrustum frustum, float songPos, Project project, TrackProps trackProps)
        {
            foreach (var bbox in _geo.bboxes)
            {
                BoundingBoxEx bb = bbox.clone();
                bb.scale(new Vector3(project.ViewWidthQnScale, 1, 1));

                Vector3 posOffset = project.getSpatialNormPosOffset(trackProps);
                posOffset.X -= songPos;
                bb.translate(posOffset);

                if (bb.intersects(frustum))
                    return true;
            }
            return false;
        }
    }
}
