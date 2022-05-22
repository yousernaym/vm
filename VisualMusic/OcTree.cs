using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace VisualMusic
{
    public class OcTree<Geo> where Geo : VisualMusic.Geo
    {
        int refCOunt = 0;
        protected OcTree<Geo>[] _nodes = null;
        protected bool _isEmpty = true;
        protected BoundingBox _bbox;
        protected List<BoundingBox> _objects = new List<BoundingBox>();
        public Geo _geo;
        //protected VertexBuffer vertexBuffers;
        protected Vector3 _minSize;

        public delegate void CreateGeoChunk(out Geo geo, BoundingBox bbox, Midi.Track midiTrack, TrackProps trackProps, MaterialProps texMaterial);
        public delegate void DrawGeoChunk(Geo geo);
        CreateGeoChunk _createGeoChunk;
        DrawGeoChunk _drawGeoChunk;

        //public OcTree()
        //{

        //}

        public OcTree(Vector3 minPos, Vector3 size, Vector3 minSize, CreateGeoChunk createGeoChunk, DrawGeoChunk drawGeoChunk)
        {
            _minSize = minSize;
            _bbox = new BoundingBox(minPos, minPos + size);
            _createGeoChunk = createGeoChunk;
            _drawGeoChunk = drawGeoChunk;
            if (size.X <= minSize.X && size.Y <= minSize.Y && size.Z <= minSize.Z)
                return;

            #region Create sub-nodes
            /*
			_nodes = new OcTree<Geo>[8];
			Vector3 subNodeSize;
			subNodeSize.X = size.X > minSize.X ? size.X / 2 : size.X;
			subNodeSize.Y = size.Y > minSize.Y ? size.Y / 2 : size.Y;
			subNodeSize.Z = size.Z > minSize.Z ? size.Z / 2 : size.Z;

			Vector3 subNodeSizeX = new Vector3(subNodeSize.X, 0, 0);
			Vector3 subNodeSizeY = new Vector3(0, subNodeSize.Y, 0);
			Vector3 subNodeSizeZ = new Vector3(0, 0, subNodeSize.Z);

			for (int i = 0; i < 2; i++)
			{
				int index = i * 2;
				_nodes[index] = new OcTree<Geo>(minPos, subNodeSize, minSize, createGeoChunk, drawGeoChunk); //top-left
				if (subNodeSize.X < size.X)
					_nodes[index + 1] = new OcTree<Geo>(minPos + subNodeSizeX, subNodeSize, minSize, createGeoChunk, drawGeoChunk); //top-right
				if (subNodeSize.Y < size.Y)
					_nodes[index + 2] = new OcTree<Geo>(minPos + subNodeSizeY, subNodeSize, minSize, createGeoChunk, drawGeoChunk); //bottom-left
				if (subNodeSize.X < size.X && subNodeSize.Y < size.Y)
					_nodes[index + 3] = new OcTree<Geo>(minPos + subNodeSizeX + subNodeSizeY, subNodeSize, minSize, createGeoChunk, drawGeoChunk); //bottom-right
				if (subNodeSize.Z == size.Z)
					break;
				minPos.Z += subNodeSize.Z;
			}
			*/
            #endregion
        }

        //public OcTree(Vector3 minPos, Vector3 size, Vector3 minSize, CreateGeoChunk createGeoChunk)
        //{
        //	return new OcTree(minPos, size, minPos, createGeoChunk);
        //}

        public bool addObject(BoundingBox obj)
        {
            if (!obj.Intersects(_bbox))
                return false;
            _isEmpty = false;
            _objects.Add(obj);
            foreach (var node in _nodes)
                node.addObject(obj);
            return true;
        }

        public void createGeo(Midi.Track midiTrack, TrackProps trackProps, TrackProps globalTrackProps, MaterialProps texMaterial)
        {
            _createGeoChunk(out _geo, _bbox, midiTrack, trackProps, texMaterial);
            return;
            if (_nodes != null)
            {
                foreach (var node in _nodes)
                {
                    if (node == null)
                        continue;
                    node.createGeo(midiTrack, trackProps, globalTrackProps, texMaterial);
                }
            }
        }

        public void drawGeo(Camera cam)
        {
            //if (!cam.Frustum.Intersects(_bbox))
            //return;
            _drawGeoChunk(_geo);
            return;
            if (_nodes == null)
                _drawGeoChunk(_geo);
            else
            {
                foreach (var node in _nodes)
                {
                    if (node != null)
                        node.drawGeo(cam);
                }
            }
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
            if (_nodes != null)
            {
                foreach (var node in _nodes)
                {
                    if (node != null)
                        node.Dispose();
                }
            }
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
