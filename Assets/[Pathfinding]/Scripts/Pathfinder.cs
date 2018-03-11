﻿using System.Collections.Generic;
using System.Linq;
using BrunoMikoski.Pahtfinding.Grid;
using Priority_Queue;
using UnityEngine;
using UnityEngine.Profiling;

namespace BrunoMikoski.Pahtfinding
{
    public static class Pathfinder
    {
        private static GridController gridController;

        private static FastPriorityQueue<Tile> openListPriorityQueue;
        private static Dictionary<int, Tile> tileIndexToTileObjectOpen = new Dictionary<int, Tile>();
        private static HashSet<Tile> closedList = new HashSet<Tile>();
        private static Tile[] neighbors = new Tile[4];

        public static void Initialize( GridController targetGridController )
        {
            gridController = targetGridController;
            openListPriorityQueue = new FastPriorityQueue<Tile>( gridController.GridSizeX * gridController.GridSizeY );
        }

        public static List<Vector2Int> GetPath( Vector2Int from, Vector2Int to )
        {
            closedList.Clear();

            int fromIndex = gridController.TilePosToIndex( from.x, from.y );
            int toIndex = gridController.TilePosToIndex( to.x, to.y );

            Tile initialTile = gridController.Tiles[fromIndex];
            Tile destinationTile = gridController.Tiles[toIndex];

            openListPriorityQueue.Enqueue( initialTile, 0 );
            tileIndexToTileObjectOpen.Add( initialTile.Index, initialTile );

            while ( openListPriorityQueue.Count > 0 )
            {
                Tile currentTile = openListPriorityQueue.Dequeue();
                tileIndexToTileObjectOpen.Remove( currentTile.Index );

                closedList.Add( currentTile );

                if ( currentTile == destinationTile )
                    break;

                UpdateNeighbors( currentTile );

                foreach ( Tile neighbourPathTile in neighbors )
                {
                    if ( neighbourPathTile == null )
                        continue;

                    if ( closedList.Contains( neighbourPathTile ) )
                        continue;

                    float movementCostToNeighbour = currentTile.GCost + GetDistance( currentTile, neighbourPathTile );
                    bool isAtOpenList = tileIndexToTileObjectOpen.ContainsKey( neighbourPathTile.Index );
                    if ( movementCostToNeighbour < neighbourPathTile.GCost || !isAtOpenList )
                    {
                        neighbourPathTile.SetGCost( movementCostToNeighbour );
                        neighbourPathTile.SetHCost( GetDistance( neighbourPathTile, destinationTile ) );
                        neighbourPathTile.SetParent( currentTile );

                        if ( !isAtOpenList )
                        {
                            openListPriorityQueue.Enqueue( neighbourPathTile,
                                                           neighbourPathTile.FCost + neighbourPathTile.HCost );
                            tileIndexToTileObjectOpen.Add( neighbourPathTile.Index, neighbourPathTile );
                        }
                    }
                }
            }

            Tile tile = closedList.Last();
            List<Vector2Int> finalPath = new List<Vector2Int>();
            while ( tile != initialTile )
            {
                finalPath.Add( new Vector2Int( tile.PositionX, tile.PositionY ) );
                tile = tile.Parent;
            }

            finalPath.Reverse();
            
            openListPriorityQueue.Clear();
            tileIndexToTileObjectOpen.Clear();
            return finalPath;
        }


        private static float GetDistance( Tile targetFromTile, Tile targetToTile )
        {
            return (targetFromTile.PositionX - targetToTile.PositionY) *
                   (targetFromTile.PositionX - targetToTile.PositionX) +
                   (targetFromTile.PositionY - targetToTile.PositionY) *
                   (targetFromTile.PositionY - targetToTile.PositionY);
        }

        private static Tile[] UpdateNeighbors( Tile targetTile )
        {
            neighbors[0] = GetNeighborAtDirection( targetTile, NeighborDirection.LEFT );
            neighbors[1] = GetNeighborAtDirection( targetTile, NeighborDirection.TOP );
            neighbors[2] = GetNeighborAtDirection( targetTile, NeighborDirection.RIGHT );
            neighbors[3] = GetNeighborAtDirection( targetTile, NeighborDirection.DOWN );

            return neighbors;
        }

        private static Tile GetNeighborAtDirection( Tile targetTile, NeighborDirection targetDirection )
        {
            int positionX;
            int positionY;

            GetNeighbourPosition( targetTile, targetDirection, out positionX, out positionY );
            if ( !gridController.IsValidTilePosition( positionX, positionY ) )
                return null;
            
            int neighborIndex = gridController.TilePosToIndex( positionX, positionY );

            return gridController.Tiles[neighborIndex];
        }

        private static void GetNeighbourPosition( Tile targetTile, NeighborDirection targetDirection ,out int targetPositionX, out int targetPositionY)
        {
            targetPositionX = targetTile.PositionX;
            targetPositionY = targetTile.PositionY;
            switch ( targetDirection )
            {
                case NeighborDirection.LEFT:
                    targetPositionX -= 1;
                    break;
                case NeighborDirection.TOP:
                    targetPositionY += 1;
                    break;
                case NeighborDirection.RIGHT:
                    targetPositionX += 1;
                    break;
                case NeighborDirection.DOWN:
                    targetPositionY -= 1;
                    break;
            }
        }
    }
}
