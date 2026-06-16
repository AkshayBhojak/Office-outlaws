using System;
using Game.Enemy;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Game
{
    [CustomEditor(typeof(Node), true)]
    public class NodeEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (Application.isPlaying)
                return;

            var node = target as Node;
            var path = AssetDatabase.GetAssetPath(node);
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);

            foreach (var state in controller.layers[0].stateMachine.states)
            {
                if (Array.IndexOf(state.state.behaviours, node) > -1)
                {
                    state.state.name = (node + " " + state.position.y).Replace(".", ",");
                }
            }
        }
    }
}