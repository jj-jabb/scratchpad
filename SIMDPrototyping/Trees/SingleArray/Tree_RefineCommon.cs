﻿using BEPUutilities.DataStructures;
using BEPUutilities.ResourceManagement;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SIMDPrototyping.Trees.SingleArray
{
    internal struct SubtreeHeapEntry
    {
        public int Index;
        public float Cost;
    }
    unsafe internal struct SubtreeBinaryHeap
    {
        public SubtreeHeapEntry* Entries;
        public int Count;

        public SubtreeBinaryHeap(SubtreeHeapEntry* entries)
        {
            Entries = entries;
            Count = 0;
        }

    
        public unsafe void Insert(Node* node, Node* nodes, ref QuickList<int> subtrees)
        {
            var children = &node->ChildA;
            var bounds = &node->A;
            for (int childIndex = 0; childIndex < node->ChildCount; ++childIndex)
            {
                if (children[childIndex] >= 0)
                {
                    int index = Count;
                    var cost = Tree.ComputeBoundsMetric(ref bounds[childIndex]);
                    ++Count;

                    //Sift up.
                    while (index > 0)
                    {
                        var parentIndex = (index - 1) >> 1;
                        var parent = Entries + parentIndex;
                        if (parent->Cost < cost)
                        {
                            //Pull the parent down.
                            Entries[index] = *parent;
                            index = parentIndex;
                        }
                        else
                        {
                            //Found the insertion spot.
                            break;
                        }
                    }
                    var entry = Entries + index;
                    entry->Index = children[childIndex];
                    entry->Cost = cost;
                    
                }
                else
                {
                    //Immediately add leaf nodes.
                    subtrees.Add(children[childIndex]);
                }
            }

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void Pop(out SubtreeHeapEntry entry)
        {
            entry = Entries[0];
            --Count;
            var cost = Entries[Count].Cost;
            
            //Pull the elements up to fill in the gap.
            int index = 0;
            while (true)
            {
                var childIndexA = (index << 1) + 1;
                var childIndexB = (index << 1) + 2;
                if (childIndexB < Count)
                {
                    //Both children are available.
                    //Try swapping with the largest one.
                    var childA = Entries + childIndexA;
                    var childB = Entries + childIndexB;
                    if (childA->Cost > childB->Cost)
                    {
                        if (cost > childA->Cost)
                        {
                            break;
                        }
                        Entries[index] = Entries[childIndexA];
                        index = childIndexA;
                    }
                    else
                    {
                        if (cost > childB->Cost)
                        {
                            break;
                        }
                        Entries[index] = Entries[childIndexB];
                        index = childIndexB;
                    }
                }
                else if (childIndexA < Count)
                {
                    //Only one child was available.
                    var childA = Entries + childIndexA;
                    if (cost > childA->Cost)
                    {
                        break;
                    }
                    Entries[index] = Entries[childIndexA];
                    index = childIndexA;
                }
                else
                {
                    //The children were beyond the heap.
                    break;
                }
            }
            //Move the last entry into position.
            Entries[index] = Entries[Count];
            
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryPop(Node* nodes, ref int remainingSubtreeSpace, ref QuickList<int> subtrees, out int index, out float cost)
        {
            Debug.Assert(Count > 0);
            while (Count > 0)
            {
                //Repeatedly pop minimum until you find one that can fit.
                //Given the unique access nature, the fact that you're destroying the heap when there's not much space left doesn't matter.
                //In the event that you consume all the nodes, that just means there aren't any entries which would fit in the subtree set anymore.
                SubtreeHeapEntry entry;
                Pop(out entry);
                var node = nodes + entry.Index;
                var changeInChildCount = node->ChildCount - 1;
                if (remainingSubtreeSpace >= changeInChildCount)
                {
                    index = entry.Index;
                    cost = entry.Cost;
                    remainingSubtreeSpace -= changeInChildCount;
                    return true;
                }
                else
                {
                    //Since we won't be able to find this later, it needs to be added now.
                    subtrees.Add(entry.Index);
                }
            }
            index = -1;
            cost = -1;
            return false;
        }
    }


    partial class Tree
    {

        public unsafe void CollectSubtrees(int nodeIndex, int maximumSubtrees, ref QuickList<int> subtrees, ref QuickList<int> internalNodes, out float treeletCost)
        {

            //Collect subtrees iteratively by choosing the highest surface area subtree repeatedly.
            //This collects every child of a given node at once- the set of subtrees must not include only SOME of the children of a node.

            //(You could lift this restriction and only take some nodes, but it would complicate things. You could not simply remove
            //the parent and add its children to go deeper; it would require doing some post-fixup on the results of the construction
            //or perhaps constraining the generation process to leave room for the unaffected nodes.)

            var node = nodes + nodeIndex;
            Debug.Assert(maximumSubtrees >= node->ChildCount, "Can't only consider some of a node's children, but specified maximumSubtrees precludes the treelet root's children.");
            //All of treelet root's children are included immediately. (Follows from above requirement.)
            
            var entries = stackalloc SubtreeHeapEntry[maximumSubtrees];
            var priorityQueue = new SubtreeBinaryHeap(entries);

            priorityQueue.Insert(node, nodes, ref subtrees);

            //Cache the index of the treelet root. Later, the root will be moved to the end of the list to guarantee that it's the first node that's used.
            //This provides the guarantee that the treelet root index will not change.
            var rootIndex = internalNodes.Count;
            internalNodes.Add(nodeIndex);

            //Note that the treelet root's cost is excluded from the treeletCost.
            //That's because the treelet root cannot change.
            treeletCost = 0;
            int highestIndex;
            float highestCost;
            int remainingSubtreeSpace = maximumSubtrees - priorityQueue.Count;
            while (priorityQueue.TryPop(nodes, ref remainingSubtreeSpace, ref subtrees, out highestIndex, out highestCost))
            {
                treeletCost += highestCost;
                internalNodes.Add(highestIndex);

                //Add all the children to the set of subtrees.
                //This is safe because we pre-validated the number of children in the node.
                var expandedNode = nodes + highestIndex;
                priorityQueue.Insert(expandedNode, nodes, ref subtrees);
            }

            for (int i = 0; i < priorityQueue.Count; ++i)
            {
                subtrees.Add(priorityQueue.Entries[i].Index);
            }

            //Swap the treelet root into the last position so that the first internal node consumed is guaranteed to be the root.
            var lastIndex = internalNodes.Count - 1;
            var temp = internalNodes.Elements[lastIndex];
            internalNodes.Elements[lastIndex] = internalNodes.Elements[rootIndex];
            internalNodes.Elements[rootIndex] = temp;
        }


    }
}