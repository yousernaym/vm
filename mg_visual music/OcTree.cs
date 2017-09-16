using System;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

namespace Visual_Music
{
	public class OcTree<Geo> where Geo : IDisposable
	{
		protected OcTree<Geo>[] _nodes = new OcTree<Geo>[8];
		protected bool _isEmpty = true;
		protected BoundingBox _bbox;
		protected List<BoundingBox> _objects = new List<BoundingBox>();
		protected Geo _geo;
		//protected VertexBuffer vertexBuffers;
		protected Vector3 _minSize;

		public delegate void CreateGeoChunk(out Geo geo, BoundingBox bbox, Midi.Track midiTrack, SongDrawProps songDrawProps, TrackProps trackProps, TrackProps globalTrackProps, TrackProps texTrackProps);
		CreateGeoChunk _createGeoChunk;

		//public OcTree()
		//{

		//}

		public OcTree(Vector3 minPos, Vector3 size, Vector3 minSize, CreateGeoChunk createGeoChunk)
		{
			_minSize = minSize;
			_bbox = new BoundingBox(minPos, minPos + size);
			_createGeoChunk = createGeoChunk;
			if (size.X <= minSize.X && size.Y <= minSize.Y && size.Z <= minSize.Z)
				return;

			#region Create sub-nodes
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
				_nodes[index] = new OcTree<Geo>(minPos, subNodeSize, minSize, createGeoChunk); //top-left
				if (subNodeSize.X < size.X)
					_nodes[index + 1] = new OcTree<Geo>(minPos + subNodeSizeX, subNodeSize, minSize, createGeoChunk); //top-right
				if (subNodeSize.Y < size.Y)
					_nodes[index + 2] = new OcTree<Geo>(minPos + subNodeSizeY, subNodeSize, minSize, createGeoChunk); //bottom-left
				if (subNodeSize.X < size.X && subNodeSize.Y < size.Y)
					_nodes[index + 3] = new OcTree<Geo>(minPos + subNodeSizeX + subNodeSizeY, subNodeSize, minSize, createGeoChunk); //bottom-right
				if (subNodeSize.Z == size.Z)
					break;
				minPos.Z += subNodeSize.Z;
			}
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

		public void createGeo(Midi.Track midiTrack, SongDrawProps songDrawProps, TrackProps trackProps, TrackProps globalTrackProps, TrackProps texTrackProps)
		{
			_createGeoChunk(out _geo, _bbox, midiTrack, songDrawProps, trackProps, globalTrackProps, texTrackProps);
			foreach (var node in _nodes)
			{
				if (node == null)
					continue;
				node.createGeo(midiTrack, songDrawProps, trackProps, globalTrackProps, texTrackProps);
			}
		}

		internal void dispose()
		{
			_geo.Dispose();
			foreach (var node in _nodes)
			{
				if (node != null)
					node.dispose();
			}
		}
	}
}