using System.Collections.Generic;
using System.Linq;
using Game.Enemy;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Game
{
    [CustomEditor(typeof(PathToNode), false)]
    [CanEditMultipleObjects]
    public sealed class PathToNodeEditor : NodeEditor
    {
        private List<AnimatorState> _states;

        private void OnEnable()
        {
            var path = AssetDatabase.GetAssetPath(target);
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);

            List<ChildAnimatorState> states = new List<ChildAnimatorState>(controller.layers[0].stateMachine.states);
            _states = states.OrderBy(temp => temp.position.y).Select(temp => temp.state).ToList();

            SceneView.duringSceneGui += SceneUpdate;
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= SceneUpdate;
        }
        
        private void SceneUpdate(SceneView sceneview)
        {
            bool isFindNode = false;

            foreach (object obj in Selection.objects)
            {
                if (!(obj is AnimatorState))
                    continue;

                var state = obj as AnimatorState;
                if (state.behaviours.Length == 0 || !(state.behaviours[0] is PathToNode))
                    continue;

                var pathNode = state.behaviours[0] as PathToNode;
                Handles.Label(pathNode.Position, _states.IndexOf(state).ToString());
                pathNode.Position = Handles.PositionHandle(pathNode.Position, Quaternion.identity);
                isFindNode = true;
            }

            if (isFindNode)
            {
                Repaint();
            }
        }
    }
}