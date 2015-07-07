﻿//#define OUTPUT
#define NODE8

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

#if NODE32
using Node = SIMDPrototyping.Trees.Baseline.Node32;
#elif NODE16
using Node = SIMDPrototyping.Trees.Baseline.Node16;
#elif NODE8
using Node = SIMDPrototyping.Trees.Baseline.Node8;
#elif NODE4
using Node = SIMDPrototyping.Trees.Baseline.Node4;
#elif NODE2
using Node = SIMDPrototyping.Trees.Baseline.Node2;
#endif


namespace SIMDPrototyping.Trees.Baseline
{

    public unsafe partial class Tree<T> : IDisposable where T : IBounded
    {
        public const int ChildrenCapacity =
#if NODE32
            32;
#elif NODE16
            16;
#elif NODE8
            8;
#elif NODE4
            4;
#elif NODE2
            2;
#endif

        unsafe struct Level
        {
            //Consider using a pointer to avoid pointless range checking.
            //Requires fixing. Use debug conditional stuff to ensure safety;
            //may want to use the array itself in debug mode too.
            //Doesn't change the other syntax really.
            public Node[] NodesArray;
            public GCHandle NodesHandle;
            public Node* Nodes;
            public int Count;

            public int Add(ref Node node)
            {
                if (Count == NodesArray.Length)
                {
                    Debug.Assert(NodesHandle.IsAllocated);
                    NodesHandle.Free();
                    var newNodes = new Node[NodesArray.Length * 2];
                    Array.Copy(NodesArray, newNodes, NodesArray.Length);
                    NodesArray = newNodes;
                    NodesHandle = GCHandle.Alloc(NodesArray, GCHandleType.Pinned);
                    Nodes = (Node*)NodesHandle.AddrOfPinnedObject();
                }
                NodesArray[Count] = node;
                return Count++;
            }

            internal void Initialize(long size)
            {
                Debug.Assert(!NodesHandle.IsAllocated);
                NodesArray = new Node[size];
                NodesHandle = GCHandle.Alloc(NodesArray, GCHandleType.Pinned);
                Nodes = (Node*)NodesHandle.AddrOfPinnedObject();

            }
        }
        Level[] Levels;


        struct Leaf
        {
            public T Bounded;
            /// <summary>
            /// Which level the leaf is in.
            /// </summary>
            public int LevelIndex;
            /// <summary>
            /// Which node within the level the leaf is in.
            /// </summary>
            public int NodeIndex;
            /// <summary>
            /// Which child within the node the leaf is in.
            /// </summary>
            public int ChildIndex;
        }
        Leaf[] leaves;
        int leafCount;

        public int LeafCount
        {
            get
            {
                return leafCount;
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        int AddLeaf(T leaf, int levelIndex, int nodeIndex, int childIndex)
        {
            if (leafCount == leaves.Length)
            {
                var newLeaves = new Leaf[leafCount * 2];
                Array.Copy(leaves, newLeaves, leafCount);
                leaves = newLeaves;
            }
            leaves[leafCount].Bounded = leaf;
            leaves[leafCount].LevelIndex = levelIndex;
            leaves[leafCount].NodeIndex = nodeIndex;
            leaves[LeafCount].ChildIndex = childIndex;
            return leafCount++;
        }

        private void EnsureLevel(int nextLevel)
        {
            if (nextLevel >= Levels.Length)
            {
                var newLevels = new Level[Levels.Length * 2];
                Array.Copy(Levels, newLevels, Levels.Length);
                for (int i = Levels.Length; i < newLevels.Length; ++i)
                {
                    //Assume the same size for subsequent levels. While they could grow exponentially,
                    //typically the tree will not be perfectly balanced.
                    newLevels[i].Initialize(Levels[Levels.Length - 1].NodesArray.Length);
                }
                Levels = newLevels;
            }
            if (nextLevel > maximumDepth)
                maximumDepth = nextLevel;

        }




        int maximumDepth;
        /// <summary>
        /// Gets the index of the deepest tree layer that contains any nodes.
        /// </summary>
        public int MaximumDepth
        {
            get
            {
                return maximumDepth;
            }
        }

        public unsafe Tree(int initialLeafCapacity = 4096, int initialTreeDepth = 8)
        {
            if (initialTreeDepth <= 0)
                throw new ArgumentException("Initial tree depth must be positive.");
            if (initialLeafCapacity <= 0)
                throw new ArgumentException("Initial leaf capacity must be positive.");

            Levels = new Level[initialTreeDepth];
            var maxExponent = Math.Log(1e10, ChildrenCapacity);
            for (int i = 0; i < Levels.Length; ++i)
            {
                //The worst case scenario is one leaf node per node.
                Levels[i].Initialize(Math.Min(initialLeafCapacity, (long)Math.Pow(ChildrenCapacity, Math.Min(maxExponent, i))));
            }
            InitializeNode(out Levels[0].Nodes[0]);
            Levels[0].Count = 1;

            leaves = new Leaf[initialLeafCapacity];

        }

        //Node initialNode;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void InitializeNode(out Node node)
        {
#if NODE32
            node.A = new BoundingBox { Min = new Vector3(float.MaxValue), Max = new Vector3(-float.MaxValue) };
            node.B = node.A;
            node.C = node.A;
            node.D = node.A;
            node.E = node.A;
            node.F = node.A;
            node.G = node.A;
            node.H = node.A;
            node.I = node.A;
            node.J = node.A;
            node.K = node.A;
            node.L = node.A;
            node.M = node.A;
            node.N = node.A;
            node.O = node.A;
            node.P = node.A;
            node.A2 = node.A;
            node.B2 = node.A;
            node.C2 = node.A;
            node.D2 = node.A;
            node.E2 = node.A;
            node.F2 = node.A;
            node.G2 = node.A;
            node.H2 = node.A;
            node.I2 = node.A;
            node.J2 = node.A;
            node.K2 = node.A;
            node.L2 = node.A;
            node.M2 = node.A;
            node.N2 = node.A;
            node.O2 = node.A;
            node.P2 = node.A;
            node.ChildA = -1;
            node.ChildB = -1;
            node.ChildC = -1;
            node.ChildD = -1;
            node.ChildE = -1;
            node.ChildF = -1;
            node.ChildG = -1;
            node.ChildH = -1;
            node.ChildI = -1;
            node.ChildJ = -1;
            node.ChildK = -1;
            node.ChildL = -1;
            node.ChildM = -1;
            node.ChildN = -1;
            node.ChildO = -1;
            node.ChildP = -1;
            node.ChildA2 = -1;
            node.ChildB2 = -1;
            node.ChildC2 = -1;
            node.ChildD2 = -1;
            node.ChildE2 = -1;
            node.ChildF2 = -1;
            node.ChildG2 = -1;
            node.ChildH2 = -1;
            node.ChildI2 = -1;
            node.ChildJ2 = -1;
            node.ChildK2 = -1;
            node.ChildL2 = -1;
            node.ChildM2 = -1;
            node.ChildN2 = -1;
            node.ChildO2 = -1;
            node.ChildP2 = -1;
            node.ChildCount = 0;
#elif NODE16
            node.A = new BoundingBox { Min = new Vector3(float.MaxValue), Max = new Vector3(-float.MaxValue) };
            node.B = node.A;
            node.C = node.A;
            node.D = node.A;
            node.E = node.A;
            node.F = node.A;
            node.G = node.A;
            node.H = node.A;
            node.I = node.A;
            node.J = node.A;
            node.K = node.A;
            node.L = node.A;
            node.M = node.A;
            node.N = node.A;
            node.O = node.A;
            node.P = node.A;
            node.ChildA = -1;
            node.ChildB = -1;
            node.ChildC = -1;
            node.ChildD = -1;
            node.ChildE = -1;
            node.ChildF = -1;
            node.ChildG = -1;
            node.ChildH = -1;
            node.ChildI = -1;
            node.ChildJ = -1;
            node.ChildK = -1;
            node.ChildL = -1;
            node.ChildM = -1;
            node.ChildN = -1;
            node.ChildO = -1;
            node.ChildP = -1;
            node.ChildCount = 0;
#elif NODE8
            node.A = new BoundingBox { Min = new Vector3(float.MaxValue), Max = new Vector3(-float.MaxValue) };
            node.B = node.A;
            node.C = node.A;
            node.D = node.A;
            node.E = node.A;
            node.F = node.A;
            node.G = node.A;
            node.H = node.A;
            node.ChildA = -1;
            node.ChildB = -1;
            node.ChildC = -1;
            node.ChildD = -1;
            node.ChildE = -1;
            node.ChildF = -1;
            node.ChildG = -1;
            node.ChildH = -1;
            node.ChildCount = 0;
#elif NODE4
            //could load a premade one instead.
            node.A = new BoundingBox { Min = new Vector3(float.MaxValue), Max = new Vector3(-float.MaxValue) };
            node.B = node.A;
            node.C = node.A;
            node.D = node.A;
            node.ChildA = -1;
            node.ChildB = -1;
            node.ChildC = -1;
            node.ChildD = -1;
            node.ChildCount = 0;
            //'no child' is encoded as -1. 
            //Leaf nodes are encoded as -(leafIndex + 2).
#elif NODE2
            node.A = new BoundingBox { Min = new Vector3(float.MaxValue), Max = new Vector3(-float.MaxValue) };
            node.B = node.A;
            node.ChildA = -1;
            node.ChildB = -1;
            node.ChildCount = 0;
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        int Encode(int index)
        {
            return -(index + 2);
        }

        string GetChoiceList(List<int> choices)
        {
            StringBuilder builder = new StringBuilder(choices.Count * 2);
            for (int i = 0; i < choices.Count; ++i)
            {
                builder.Append(choices[i]);
                if (i != choices.Count - 1)
                    builder.Append(", ");
            }
            return builder.ToString();
        }

        public unsafe void Insert(T leaf)
        {
            int levelIndex = 0;
            int nodeIndex = 0;
            BoundingBox box;
            leaf.GetBoundingBox(out box);
#if OUTPUT
            List<int> choices = new List<int>();
#endif
            while (true)
            {
                var level = Levels[levelIndex];
                //Which child should the leaf belong to?

                //Give the leaf to whichever node had the least volume change.
                var node = level.Nodes + nodeIndex;
                var boundingBoxes = &node->A;
                var children = &node->ChildA;
                int minimumIndex = 0;
                float minimumChange = float.MaxValue;
                BoundingBox merged = new BoundingBox();
                var max = Math.Min(ChildrenCapacity, node->ChildCount + 1);
                for (int i = 0; i < max; ++i)
                {
                    //if (children[i] == -1)
                    //{
                    //    minimumIndex = i;
                    //    merged = box;
                    //    break;
                    //}
                    var oldVolume = Math.Max(0, BoundingBox.ComputeVolume(ref boundingBoxes[i]));
                    BoundingBox mergedCandidate;
                    BoundingBox.Merge(ref boundingBoxes[i], ref box, out mergedCandidate);
                    var newVolume = BoundingBox.ComputeVolume(ref mergedCandidate);
                    var change = newVolume - oldVolume;
                    if (change < minimumChange)
                    {
                        minimumChange = change;
                        minimumIndex = i;
                        merged = mergedCandidate;
                    }
                }
#if OUTPUT
                Console.WriteLine($"Minimum index: {minimumIndex}, minimum volume increase: {minimum}");
                choices.Add(minimumIndex);
#endif

                var childIndex = children[minimumIndex];

                if (childIndex < -1)
                {
                    //It's a leaf node.
                    //Create a new internal node with the new leaf and the old leaf as children.
                    var nextLevel = levelIndex + 1;
                    //this is the only place where a new level could potentially be created.
                    EnsureLevel(nextLevel);
                    Node newNode;
                    InitializeNode(out newNode);
                    newNode.ChildCount = 2;
                    //The first child of the new node is the old leaf. Insert its bounding box.
                    newNode.A = boundingBoxes[minimumIndex];
                    newNode.ChildA = children[minimumIndex];

                    //Insert the new leaf into the second child slot.
                    newNode.B = box;
                    var newNodeIndex = Levels[nextLevel].Add(ref newNode);
                    var leafIndex = AddLeaf(leaf, nextLevel, newNodeIndex, 1);
                    Levels[nextLevel].Nodes[newNodeIndex].ChildB = Encode(leafIndex);

                    //Update the old leaf node with the new index information.
                    var oldLeafIndex = Encode(childIndex);
                    leaves[oldLeafIndex].LevelIndex = nextLevel;
                    leaves[oldLeafIndex].NodeIndex = newNodeIndex;
                    leaves[oldLeafIndex].ChildIndex = 0;

                    //Update the original node's child pointer and bounding box.
                    children[minimumIndex] = newNodeIndex;
                    boundingBoxes[minimumIndex] = merged;


#if OUTPUT
                    Console.WriteLine($"Leaf {leafIndex} merged with existing leaf.");// New Node Children: {newNode.Children}, Old Node children: {level.Nodes[nodeIndex].Children}");
                    Console.WriteLine($"Choices: {GetChoiceList(choices)}");
#endif

                    break;
                }
                if (childIndex == -1)
                {
                    //There is no child at all.
                    //Put the new leaf here.
                    ++node->ChildCount;
                    var leafIndex = AddLeaf(leaf, levelIndex, nodeIndex, minimumIndex);
                    children[minimumIndex] = Encode(leafIndex);
                    boundingBoxes[minimumIndex] = merged;


#if OUTPUT
                    Console.WriteLine($"Leaf {leafIndex} inserted in empty slot.");
                    Console.WriteLine($"Choices: {GetChoiceList(choices)}");
#endif
                    break;
                }
                //It's an internal node. Traverse to the next node.
                boundingBoxes[minimumIndex] = merged;
                nodeIndex = children[minimumIndex];
                ++levelIndex;

            }
        }




        public unsafe void Refit()
        {
            //Update the bounding boxes of every leaf-owner.
            //Note the scalar-ness of this. It seems like there should exist some way to vectorize it properly, though it may require changing things around.
            for (int i = 0; i < leafCount; ++i)
            {
                leaves[i].Bounded.GetBoundingBox(out (&Levels[leaves[i].LevelIndex].Nodes[leaves[i].NodeIndex].A)[leaves[i].ChildIndex]);
                //Console.WriteLine($"index reached: {i}, child index: {leaves[i].ChildIndex}, level: {leaves[i].LevelIndex}, node: { leaves[i].NodeIndex}");


            }
            //Go through each level, refitting as you go.
            //Note that the deepest level is skipped. It does not need to be tested; it's all leaves that were already updated.
            for (int levelIndex = maximumDepth - 1; levelIndex >= 0; --levelIndex)
            {
                var level = Levels[levelIndex];
                var nextLevel = Levels[levelIndex + 1];
                //consider testing caching Levels[levelIndex]. It may have a minor effect.
                for (int nodeIndex = 0; nodeIndex < level.Count; ++nodeIndex)
                {
                    var node = level.Nodes + nodeIndex;
                    var boundingBoxPointer = &level.Nodes[nodeIndex].A;
                    var children = &node->ChildA;
                    var childCount = node->ChildCount;
                    for (int childIndex = 0; childIndex < childCount; ++childIndex)
                    {
                        var childNodeIndex = children[childIndex];
                        //if (childNodeIndex < -1)
                        //{
                        //    var leafIndex = Encode(childNodeIndex);
                        //    if (leafIndex > LeafCount)
                        //    {
                        //        throw new Exception($"Bad leaf index {leafIndex} > {LeafCount}");
                        //    }
                        //}
                        if (childNodeIndex >= 0)
                        {
                            //if (childNodeIndex >= nextLevel.Count)
                            //{
                            //    throw new Exception($"Node ({levelIndex}, {nodeIndex}) has bad child node index: {childNodeIndex} > {nextLevel.Count}");
                            //}
                            var nextChildCount = nextLevel.Nodes[childNodeIndex].ChildCount;
                            var boundingBoxSlot = boundingBoxPointer + childIndex;
                            var childBoundingBoxes = &nextLevel.Nodes[childNodeIndex].A;
                            *boundingBoxSlot = childBoundingBoxes[0];
                            for (int i = 1; i < nextChildCount; ++i)
                            {
                                BoundingBox.Merge(ref *boundingBoxSlot, ref childBoundingBoxes[i], out *boundingBoxSlot);
                            }

                        }

                    }
                }
            }
        }




        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        unsafe void Test<TResultList>(TraversalTarget* stack, ref int count, int stackCapacity, int level,
            ref BoundingBox query, Node* node,
            ref TResultList results) where TResultList : IList<T>
        {
            var boundingBoxes = &node->A;
            var children = &node->ChildA;
            for (int i = 0; i < node->ChildCount; ++i)
            {
                if (BoundingBox.Intersects(ref query, ref boundingBoxes[i]))
                {
                    if (children[i] >= 0)
                    {
                        stack[count++] = new TraversalTarget { Level = level + 1, Node = children[i] };
                    }
                    else
                    {
                        results.Add(leaves[Encode(children[i])].Bounded);
                    }
                }

            }
        }



        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void Query<TResultList>(ref BoundingBox boundingBox, ref TResultList results) where TResultList : IList<T>
        {
            //TODO: could optimize this by keeping the next target out of the stack.
            var stackCapacity = (ChildrenCapacity - 1) * maximumDepth + 1;
            var stack = stackalloc TraversalTarget[stackCapacity];
            int count = 0;

            var boundingBoxWide = new BoundingBoxWide(ref boundingBox);
            Test(stack, ref count, stackCapacity, 0, ref boundingBox, Levels[0].Nodes, ref results);

            while (count > 0)
            {
                --count;
                var target = stack[count];

                Test(stack, ref count, stackCapacity, target.Level, ref boundingBox, Levels[target.Level].Nodes + target.Node, ref results);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        unsafe void TestRecursive<TResultList>(int level, int nodeIndex,
            ref BoundingBox query,
            ref TResultList results) where TResultList : IList<T>
        {
            var node = (Levels[level].Nodes + nodeIndex);
            var boundingBoxes = &node->A;
            var children = &node->ChildA;
            var childCount = node->ChildCount;
            var nextLevel = level + 1;
            //int INTERSECTEDCOUNT = 0;
            for (int i = 0; i < childCount; ++i)
            {
                if (BoundingBox.Intersects(ref query, ref boundingBoxes[i]))
                {
                    //++INTERSECTEDCOUNT;
                    if (children[i] >= 0)
                    {
                        TestRecursive(nextLevel, children[i], ref query, ref results);
                    }
                    else
                    {
                        results.Add(leaves[Encode(children[i])].Bounded);
                    }
                }
            }
            //Console.WriteLine($"Level {level}, intersected count: {INTERSECTEDCOUNT}");
        }


#if NODE16
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        unsafe void TestRecursive16<TResultList>(int level, int nodeIndex,
            ref BoundingBox query,
            ref TResultList results) where TResultList : IList<T>
        {
            var node = (Levels[level].Nodes + nodeIndex);
            var childCount = node->ChildCount;


            if (childCount < 1)
                return;
            var nextLevel = level + 1;
            bool a, b, c, d, e, f, g, h,
                 i, j, k, l, m, n, o, p;
            a = BoundingBox.Intersects(ref query, ref node->A);
            b = BoundingBox.Intersects(ref query, ref node->B);
            c = BoundingBox.Intersects(ref query, ref node->C);



            if (a)
            {
                if (node->ChildA >= 0)
                {
                    TestRecursive16(nextLevel, node->ChildA, ref query, ref results);
                }
                else
                {
                    results.Add(leaves[Encode(node->ChildA)].Bounded);
                }
            }
            if (childCount < 2)
                return;
            if (b)
            {
                if (node->ChildB >= 0)
                {
                    TestRecursive16(nextLevel, node->ChildB, ref query, ref results);
                }
                else
                {
                    results.Add(leaves[Encode(node->ChildB)].Bounded);
                }
            }
            if (childCount < 3)
                return;
            d = BoundingBox.Intersects(ref query, ref node->D);
            if (c)
            {
                if (node->ChildC >= 0)
                {
                    TestRecursive16(nextLevel, node->ChildC, ref query, ref results);
                }
                else
                {
                    results.Add(leaves[Encode(node->ChildC)].Bounded);
                }
            }
            if (childCount < 4)
                return;
            e = BoundingBox.Intersects(ref query, ref node->E);
            if (d)
            {
                if (node->ChildD >= 0)
                {
                    TestRecursive16(nextLevel, node->ChildD, ref query, ref results);
                }
                else
                {
                    results.Add(leaves[Encode(node->ChildD)].Bounded);
                }
            }
            if (childCount < 5)
                return;
            f = BoundingBox.Intersects(ref query, ref node->F);
            if (e)
            {
                if (node->ChildE >= 0)
                {
                    TestRecursive16(nextLevel, node->ChildE, ref query, ref results);
                }
                else
                {
                    results.Add(leaves[Encode(node->ChildE)].Bounded);
                }
            }
            if (childCount < 6)
                return;
            g = BoundingBox.Intersects(ref query, ref node->G);
            if (f)
            {
                if (node->ChildF >= 0)
                {
                    TestRecursive16(nextLevel, node->ChildF, ref query, ref results);
                }
                else
                {
                    results.Add(leaves[Encode(node->ChildF)].Bounded);
                }
            }
            if (childCount < 7)
                return;
            h = BoundingBox.Intersects(ref query, ref node->H);
            if (g)
            {
                if (node->ChildG >= 0)
                {
                    TestRecursive16(nextLevel, node->ChildG, ref query, ref results);
                }
                else
                {
                    results.Add(leaves[Encode(node->ChildG)].Bounded);
                }
            }
            if (childCount < 8)
                return;
            i = BoundingBox.Intersects(ref query, ref node->I);
            if (h)
            {
                if (node->ChildH >= 0)
                {
                    TestRecursive16(nextLevel, node->ChildH, ref query, ref results);
                }
                else
                {
                    results.Add(leaves[Encode(node->ChildH)].Bounded);
                }
            }
            if (childCount < 9)
                return;
            j = BoundingBox.Intersects(ref query, ref node->J);
            if (i)
            {
                if (node->ChildI >= 0)
                {
                    TestRecursive16(nextLevel, node->ChildI, ref query, ref results);
                }
                else
                {
                    results.Add(leaves[Encode(node->ChildI)].Bounded);
                }
            }
            if (childCount < 10)
                return;
            k = BoundingBox.Intersects(ref query, ref node->K);
            if (j)
            {
                if (node->ChildJ >= 0)
                {
                    TestRecursive16(nextLevel, node->ChildJ, ref query, ref results);
                }
                else
                {
                    results.Add(leaves[Encode(node->ChildJ)].Bounded);
                }
            }
            if (childCount < 11)
                return;
            l = BoundingBox.Intersects(ref query, ref node->L);
            if (k)
            {
                if (node->ChildK >= 0)
                {
                    TestRecursive16(nextLevel, node->ChildK, ref query, ref results);
                }
                else
                {
                    results.Add(leaves[Encode(node->ChildK)].Bounded);
                }
            }
            if (childCount < 12)
                return;
            m = BoundingBox.Intersects(ref query, ref node->M);
            if (l)
            {
                if (node->ChildL >= 0)
                {
                    TestRecursive16(nextLevel, node->ChildL, ref query, ref results);
                }
                else
                {
                    results.Add(leaves[Encode(node->ChildL)].Bounded);
                }
            }
            if (childCount < 13)
                return;
            n = BoundingBox.Intersects(ref query, ref node->N);
            if (m)
            {
                if (node->ChildM >= 0)
                {
                    TestRecursive16(nextLevel, node->ChildM, ref query, ref results);
                }
                else
                {
                    results.Add(leaves[Encode(node->ChildM)].Bounded);
                }
            }
            if (childCount < 14)
                return;
            o = BoundingBox.Intersects(ref query, ref node->O);
            if (n)
            {
                if (node->ChildN >= 0)
                {
                    TestRecursive16(nextLevel, node->ChildN, ref query, ref results);
                }
                else
                {
                    results.Add(leaves[Encode(node->ChildN)].Bounded);
                }
            }
            if (childCount < 15)
                return;
            p = BoundingBox.Intersects(ref query, ref node->P);
            if (o)
            {
                if (node->ChildO >= 0)
                {
                    TestRecursive16(nextLevel, node->ChildO, ref query, ref results);
                }
                else
                {
                    results.Add(leaves[Encode(node->ChildO)].Bounded);
                }
            }
            if (childCount < 16)
                return;
            if (p)
            {
                if (node->ChildP >= 0)
                {
                    TestRecursive16(nextLevel, node->ChildP, ref query, ref results);
                }
                else
                {
                    results.Add(leaves[Encode(node->ChildP)].Bounded);
                }
            }

        }
#endif

#if NODE8
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        unsafe void TestRecursive8<TResultList>(int level, int nodeIndex,
            ref BoundingBox query,
            ref TResultList results) where TResultList : IList<T>
        {
            var node = (Levels[level].Nodes + nodeIndex);
            var childCount = node->ChildCount;


            if (childCount < 1)
                return;
            var nextLevel = level + 1;
            bool a, b, c, d, e, f, g, h;
            a = BoundingBox.Intersects(ref query, ref node->A);
            b = BoundingBox.Intersects(ref query, ref node->B);
            c = BoundingBox.Intersects(ref query, ref node->C);



            if (a)
            {
                if (node->ChildA >= 0)
                {
                    TestRecursive8(nextLevel, node->ChildA, ref query, ref results);
                }
                else
                {
                    results.Add(leaves[Encode(node->ChildA)].Bounded);
                }
            }
            if (childCount < 2)
                return;
            if (b)
            {
                if (node->ChildB >= 0)
                {
                    TestRecursive8(nextLevel, node->ChildB, ref query, ref results);
                }
                else
                {
                    results.Add(leaves[Encode(node->ChildB)].Bounded);
                }
            }
            if (childCount < 3)
                return;
            d = BoundingBox.Intersects(ref query, ref node->D);
            if (c)
            {
                if (node->ChildC >= 0)
                {
                    TestRecursive8(nextLevel, node->ChildC, ref query, ref results);
                }
                else
                {
                    results.Add(leaves[Encode(node->ChildC)].Bounded);
                }
            }
            if (childCount < 4)
                return;
            e = BoundingBox.Intersects(ref query, ref node->E);
            if (d)
            {
                if (node->ChildD >= 0)
                {
                    TestRecursive8(nextLevel, node->ChildD, ref query, ref results);
                }
                else
                {
                    results.Add(leaves[Encode(node->ChildD)].Bounded);
                }
            }
            if (childCount < 5)
                return;
            f = BoundingBox.Intersects(ref query, ref node->F);
            if (e)
            {
                if (node->ChildE >= 0)
                {
                    TestRecursive8(nextLevel, node->ChildE, ref query, ref results);
                }
                else
                {
                    results.Add(leaves[Encode(node->ChildE)].Bounded);
                }
            }
            if (childCount < 6)
                return;
            g = BoundingBox.Intersects(ref query, ref node->G);
            if (f)
            {
                if (node->ChildF >= 0)
                {
                    TestRecursive8(nextLevel, node->ChildF, ref query, ref results);
                }
                else
                {
                    results.Add(leaves[Encode(node->ChildF)].Bounded);
                }
            }
            if (childCount < 7)
                return;
            h = BoundingBox.Intersects(ref query, ref node->H);
            if (g)
            {
                if (node->ChildG >= 0)
                {
                    TestRecursive8(nextLevel, node->ChildG, ref query, ref results);
                }
                else
                {
                    results.Add(leaves[Encode(node->ChildG)].Bounded);
                }
            }
            if (childCount < 8)
                return;
            if (h)
            {
                if (node->ChildH >= 0)
                {
                    TestRecursive8(nextLevel, node->ChildH, ref query, ref results);
                }
                else
                {
                    results.Add(leaves[Encode(node->ChildH)].Bounded);
                }
            }


        }
#endif


#if NODE4
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        unsafe void TestRecursive4<TResultList>(int level, int nodeIndex,
            ref BoundingBox query,
            ref TResultList results) where TResultList : IList<T>
        {
            var node = (Levels[level].Nodes + nodeIndex);
            var childCount = node->ChildCount;


            if (childCount < 1)
                return;
            var nextLevel = level + 1;
            bool a, b, c, d;
            a = BoundingBox.Intersects(ref query, ref node->A);
            b = BoundingBox.Intersects(ref query, ref node->B);
            c = BoundingBox.Intersects(ref query, ref node->C);


            if (a)
            {
                if (node->ChildA >= 0)
                {
                    TestRecursive4(nextLevel, node->ChildA, ref query, ref results);
                }
                else
                {
                    results.Add(leaves[Encode(node->ChildA)].Bounded);
                }
            }
            if (childCount < 2)
                return;
            d = BoundingBox.Intersects(ref query, ref node->D);
            if (b)
            {
                if (node->ChildB >= 0)
                {
                    TestRecursive4(nextLevel, node->ChildB, ref query, ref results);
                }
                else
                {
                    results.Add(leaves[Encode(node->ChildB)].Bounded);
                }
            }
            if (childCount < 3)
                return;
            if (c)
            {
                if (node->ChildC >= 0)
                {
                    TestRecursive4(nextLevel, node->ChildC, ref query, ref results);
                }
                else
                {
                    results.Add(leaves[Encode(node->ChildC)].Bounded);
                }
            }
            if (childCount < 4)
                return;
            if (d)
            {
                if (node->ChildD >= 0)
                {
                    TestRecursive4(nextLevel, node->ChildD, ref query, ref results);
                }
                else
                {
                    results.Add(leaves[Encode(node->ChildD)].Bounded);
                }
            }


        }
#endif

#if NODE4
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        unsafe void TestRecursive4Switch<TResultList>(int level, int nodeIndex,
            ref BoundingBox query,
            ref TResultList results) where TResultList : IList<T>
        {
            var node = (Levels[level].Nodes + nodeIndex);
            var childCount = node->ChildCount;

            switch (childCount)
            {
                case 1:
                    {
                        bool a = BoundingBox.Intersects(ref query, ref node->A);

                        var nextLevel = level + 1;
                        if (a)
                        {
                            if (node->ChildA >= 0)
                            {
                                TestRecursive4Switch(nextLevel, node->ChildA, ref query, ref results);
                            }
                            else
                            {
                                results.Add(leaves[Encode(node->ChildA)].Bounded);
                            }
                        }
                    }
                    break;
                case 2:
                    {
                        bool a, b;
                        a = BoundingBox.Intersects(ref query, ref node->A);
                        b = BoundingBox.Intersects(ref query, ref node->B);

                        var nextLevel = level + 1;
                        if (a)
                        {
                            if (node->ChildA >= 0)
                            {
                                TestRecursive4Switch(nextLevel, node->ChildA, ref query, ref results);
                            }
                            else
                            {
                                results.Add(leaves[Encode(node->ChildA)].Bounded);
                            }
                        }
                        if (b)
                        {
                            if (node->ChildB >= 0)
                            {
                                TestRecursive4Switch(nextLevel, node->ChildB, ref query, ref results);
                            }
                            else
                            {
                                results.Add(leaves[Encode(node->ChildB)].Bounded);
                            }
                        }
                    }
                    break;
                case 3:
                    {
                        bool a, b, c;
                        a = BoundingBox.Intersects(ref query, ref node->A);
                        b = BoundingBox.Intersects(ref query, ref node->B);
                        c = BoundingBox.Intersects(ref query, ref node->C);

                        var nextLevel = level + 1;
                        if (a)
                        {
                            if (node->ChildA >= 0)
                            {
                                TestRecursive4Switch(nextLevel, node->ChildA, ref query, ref results);
                            }
                            else
                            {
                                results.Add(leaves[Encode(node->ChildA)].Bounded);
                            }
                        }
                        if (b)
                        {
                            if (node->ChildB >= 0)
                            {
                                TestRecursive4Switch(nextLevel, node->ChildB, ref query, ref results);
                            }
                            else
                            {
                                results.Add(leaves[Encode(node->ChildB)].Bounded);
                            }
                        }
                        if (c)
                        {
                            if (node->ChildC >= 0)
                            {
                                TestRecursive4Switch(nextLevel, node->ChildC, ref query, ref results);
                            }
                            else
                            {
                                results.Add(leaves[Encode(node->ChildC)].Bounded);
                            }
                        }
                    }
                    break;
                case 4:
                    {
                        bool a, b, c, d;
                        a = BoundingBox.Intersects(ref query, ref node->A);
                        b = BoundingBox.Intersects(ref query, ref node->B);
                        c = BoundingBox.Intersects(ref query, ref node->C);
                        d = BoundingBox.Intersects(ref query, ref node->D);

                        var nextLevel = level + 1;
                        if (a)
                        {
                            if (node->ChildA >= 0)
                            {
                                TestRecursive4Switch(nextLevel, node->ChildA, ref query, ref results);
                            }
                            else
                            {
                                results.Add(leaves[Encode(node->ChildA)].Bounded);
                            }
                        }
                        if (b)
                        {
                            if (node->ChildB >= 0)
                            {
                                TestRecursive4Switch(nextLevel, node->ChildB, ref query, ref results);
                            }
                            else
                            {
                                results.Add(leaves[Encode(node->ChildB)].Bounded);
                            }
                        }
                        if (c)
                        {
                            if (node->ChildC >= 0)
                            {
                                TestRecursive4Switch(nextLevel, node->ChildC, ref query, ref results);
                            }
                            else
                            {
                                results.Add(leaves[Encode(node->ChildC)].Bounded);
                            }
                        }
                        if (d)
                        {
                            if (node->ChildD >= 0)
                            {
                                TestRecursive4Switch(nextLevel, node->ChildD, ref query, ref results);
                            }
                            else
                            {
                                results.Add(leaves[Encode(node->ChildD)].Bounded);
                            }
                        }
                    }
                    break;
            }




        }
#endif

#if NODE2
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        unsafe void TestRecursive2<TResultList>(int level, int nodeIndex,
            ref BoundingBox query,
            ref TResultList results) where TResultList : IList<T>
        {
            var node = (Levels[level].Nodes + nodeIndex);
            var childCount = node->ChildCount;


            if (childCount < 1)
                return;
            var nextLevel = level + 1;
            bool a, b;
            a = BoundingBox.Intersects(ref query, ref node->A);
            b = BoundingBox.Intersects(ref query, ref node->B);


            if (a)
            {
                if (node->ChildA >= 0)
                {
                    TestRecursive2(nextLevel, node->ChildA, ref query, ref results);
                }
                else
                {
                    results.Add(leaves[Encode(node->ChildA)].Bounded);
                }
            }
            if (childCount < 2)
                return;
            if (b)
            {
                if (node->ChildB >= 0)
                {
                    TestRecursive2(nextLevel, node->ChildB, ref query, ref results);
                }
                else
                {
                    results.Add(leaves[Encode(node->ChildB)].Bounded);
                }
            }
            


        }
#endif

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void QueryRecursive<TResultList>(ref BoundingBox boundingBox, ref TResultList results) where TResultList : IList<T>
        {
#if NODE16
            TestRecursive16(0, 0, ref boundingBox, ref results);
#elif NODE8
            TestRecursive8(0, 0, ref boundingBox, ref results);
#elif NODE4
            TestRecursive4(0, 0, ref boundingBox, ref results);
#elif NODE2
            TestRecursive2(0, 0, ref boundingBox, ref results);
#else
            TestRecursive(0, 0, ref boundingBox, ref results);
#endif
        }

        unsafe void MeasureNodeOccupancy(int levelIndex, int nodeIndex, ref int nodeCount, ref int childCount)
        {
            ++nodeCount;

            Node* node = Levels[levelIndex].Nodes + nodeIndex;
            var children = &node->ChildA;
            childCount += node->ChildCount;
            for (int i = 0; i < node->ChildCount; ++i)
            {
                if (children[i] < -1 && Encode(children[i]) >= leafCount)
                {
                    throw new InvalidOperationException("Bad leaf index.");
                }
                if (children[i] == -1)
                {
                    //Can't have a non-child within the ChildCount-specified range.
                    throw new InvalidOperationException("Bug.");
                }

                if (children[i] >= 0)
                {
                    if (children[i] >= Levels[levelIndex + 1].Count)
                    {
                        throw new InvalidOperationException("Bad node index.");
                    }
                    MeasureNodeOccupancy(levelIndex + 1, children[i], ref nodeCount, ref childCount);
                }

            }
        }

        public void MeasureNodeOccupancy(out int nodeCount, out int childCount)
        {
            nodeCount = 0;
            childCount = 0;
            MeasureNodeOccupancy(0, 0, ref nodeCount, ref childCount);

            for (int levelIndex = 0; levelIndex <= maximumDepth; ++levelIndex)
            {
                var level = Levels[levelIndex];
                var nextLevel = Levels[levelIndex + 1];
                for (int nodeIndex = 0; nodeIndex < level.Count; ++nodeIndex)
                {
                    if (level.Nodes[nodeIndex].ChildCount < 0 || level.Nodes[nodeIndex].ChildCount > ChildrenCapacity)
                        throw new Exception("too many cooks");
                    for (int childNodeIndex = 0; childNodeIndex < level.Nodes[nodeIndex].ChildCount; ++childNodeIndex)
                    {
                        
                        var nextChildNodeIndex = (&level.Nodes[nodeIndex].ChildA)[childNodeIndex];
                        if (nextChildNodeIndex > nextLevel.Count)
                        {
                            throw new Exception($"({levelIndex}, {nodeIndex}) has bad nodeIndex: {nextChildNodeIndex} > {nextLevel.Count}");
                        }
                    }
                }
            }
        }

        public void Dispose()
        {
            for (int i = 0; i <= maximumDepth; ++i)
            {
                if (Levels[i].NodesHandle.IsAllocated)
                    Levels[i].NodesHandle.Free();
            }
        }

#if DEBUG
        ~Tree()
        {
            for (int i = 0; i < Levels.Length; ++i)
            {
                Debug.Assert(!Levels[i].NodesHandle.IsAllocated, "No handle should still be allocated when the tree is finalized; implies a memory leak.");
            }
        }
#endif

    }

    struct TraversalTarget
    {
        public int Level;
        public int Node;
    }
}