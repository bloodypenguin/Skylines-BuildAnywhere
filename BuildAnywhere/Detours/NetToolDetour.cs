using System.Collections.Generic;
using System.Reflection;
using BuildAnywhere.Redirection;
using ColossalFramework;
using ColossalFramework.Math;
using UnityEngine;

namespace BuildAnywhere.Detours
{
    [TargetType(typeof(NetTool))]
    public class NetToolDetour
    {
        private static Dictionary<MethodInfo, RedirectCallsState> _redirects;

        public static void Deploy()
        {
            if (_redirects != null)
            {
                return;
            }
            _redirects = RedirectionUtil.RedirectType(typeof(NetToolDetour));
        }
        public static void Revert()
        {
            if (_redirects == null)
            {
                return;
            }
            foreach (var redirect in _redirects)
            {
                RedirectionHelper.RevertRedirect(redirect.Key, redirect.Value);
            }
            _redirects = null;
        }

        [RedirectMethod]
        private static ToolBase.ToolErrors TestNodeBuilding(BuildingInfo info, Vector3 position, Vector3 direction, ushort ignoreNode, ushort ignoreSegment, ushort ignoreBuilding, bool test, ulong[] collidingSegmentBuffer, ulong[] collidingBuildingBuffer)
        {
            Vector2 vector2_1 = new Vector2(direction.x, direction.z) * (float)((double)info.m_cellLength * 4.0 - 0.800000011920929);
            Vector2 vector2_2 = new Vector2(direction.z, -direction.x) * (float)((double)info.m_cellWidth * 4.0 - 0.800000011920929);
            if (info.m_circular)
            {
                vector2_2 *= 0.7f;
                vector2_1 *= 0.7f;
            }
            ItemClass.CollisionType collisionType = ItemClass.CollisionType.Terrain;
            if (info.m_class.m_layer == ItemClass.Layer.WaterPipes)
                collisionType = ItemClass.CollisionType.Underground;
            Vector2 vector2_3 = VectorUtils.XZ(position);
            Quad2 quad = new Quad2();
            quad.a = vector2_3 - vector2_2 - vector2_1;
            quad.b = vector2_3 - vector2_2 + vector2_1;
            quad.c = vector2_3 + vector2_2 + vector2_1;
            quad.d = vector2_3 + vector2_2 - vector2_1;
            ToolBase.ToolErrors toolErrors = ToolBase.ToolErrors.None;
            float minY = Mathf.Min(position.y, Singleton<TerrainManager>.instance.SampleRawHeightSmooth(position));
            float maxY = position.y + info.m_generatedInfo.m_size.y;
            Singleton<NetManager>.instance.OverlapQuad(quad, minY, maxY, collisionType, info.m_class.m_layer, ignoreNode, (ushort)0, ignoreSegment, collidingSegmentBuffer);
            Singleton<BuildingManager>.instance.OverlapQuad(quad, minY, maxY, collisionType, info.m_class.m_layer, ignoreBuilding, ignoreNode, (ushort)0, collidingBuildingBuffer);
            //begin mod
            //end mod
            if (!Singleton<BuildingManager>.instance.CheckLimits())
                toolErrors |= ToolBase.ToolErrors.TooManyObjects;
            return toolErrors;
        }

        [RedirectMethod]
        public static ToolBase.ToolErrors CreateNode(NetInfo info, NetTool.ControlPoint startPoint, NetTool.ControlPoint middlePoint, NetTool.ControlPoint endPoint, FastList<NetTool.NodePosition> nodeBuffer, int maxSegments, bool test, bool visualize, bool autoFix, bool needMoney, bool invert, bool switchDir, ushort relocateBuildingID, out ushort node, out ushort segment, out int cost, out int productionRate)
        {
            ushort firstNode;
            ushort lastNode;
            ToolBase.ToolErrors node1 = NetTool.CreateNode(info, startPoint, middlePoint, endPoint, nodeBuffer, maxSegments, test, true, visualize, autoFix, needMoney, invert, switchDir, relocateBuildingID, out firstNode, out lastNode, out segment, out cost, out productionRate);
            //begin mod
            if (node1 == ToolBase.ToolErrors.OutOfArea)
            {
                node1 = ToolBase.ToolErrors.None;
            }
            //end mod
            node = node1 != ToolBase.ToolErrors.None ? (ushort)0 : ((int)lastNode == 0 ? firstNode : lastNode);
            return node1;
        }
    }
}