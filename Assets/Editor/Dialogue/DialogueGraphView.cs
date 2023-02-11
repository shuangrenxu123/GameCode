using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Graphs;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using System;
using System.Linq;
using UnityEditor;

namespace DialogueEdtior
{
	public class DialogueGraphView : GraphView
	{
		public readonly Vector2 NodeSize = new(100, 150);
		private NodeSearchWindow searchWindow;
		public DialogueGraphView(EditorWindow window)
		{
			SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
			this.AddManipulator(new ContentDragger());
			this.AddManipulator(new SelectionDragger());
			this.AddManipulator(new RectangleSelector());

			AddSearchWindow(window);

			AddElement(GenerateEntryPointNode());
		}
		/// <summary>
		/// �����Ҽ��Ľڵ�˵���
		/// </summary>
        private void AddSearchWindow(EditorWindow window)
        {
            searchWindow =ScriptableObject.CreateInstance<NodeSearchWindow>();
			searchWindow.Init(this,window);
			nodeCreationRequest = context => SearchWindow.Open(new SearchWindowContext(context.screenMousePosition),searchWindow);
        }
        /// <summary>
        /// ��ʼ��һ��Port
        /// </summary>
        /// <param name="node"></param>
        /// <param name="portDirection">�˿ڷ���</param>
        /// <param name="capacity">һ���ڵ����������singleΪһ�� multiΪ���</param>
        /// <returns></returns>
        private Port GeneratePort(DialogueNode node,Direction portDirection,Port.Capacity capacity = Port.Capacity.Single)
		{
			return node.InstantiatePort(Orientation.Horizontal, portDirection, capacity, typeof(float));
		}
		/// <summary>
		/// ��ʼ��������ڽڵ�
		/// </summary>
		/// <returns></returns>
		private DialogueNode GenerateEntryPointNode()
		{
			var node = new DialogueNode()
			{
				title = "���ڵ�",
				GUID = Guid.NewGuid().ToString(),
				DialogueText = "EntryPoint",
				EntryPoint = true,
            };
			node.SetPosition(new Rect(100,200,100,150));
			var port = GeneratePort(node, Direction.Output,Port.Capacity.Multi);
			port.portName = "OutPut";
            node.outputContainer.Add(port); 
			return node;
		}
		/// <summary>
		/// ֱ����Graph������һ��Node
		/// </summary>
		/// <param name="name"></param>
		public void CreateNode(string name,Vector2 pos)
		{
			AddElement(CreatDialogueNode(name,pos));
		}
		/// <summary>
		/// ����һ���ڵ㣬��������ӵ�Graph��
		/// </summary>
		/// <param name="nodename"></param>
		/// <returns></returns>
		public DialogueNode CreatDialogueNode(string nodename,Vector2 pos)
		{
			var node = new DialogueNode()
			{
				title = nodename,
				DialogueText = nodename,
				GUID = Guid.NewGuid().ToString(),
			};
			var inputPort = GeneratePort(node, Direction.Input,Port.Capacity.Multi);
			inputPort.portName = "Input";
			node.inputContainer.Add(inputPort);

            var button = new Button(() => { AddChoicePort(node,""); });
			button.text = "������";
			node.titleContainer.Add(button);


			var tf = new TextField();
			tf.RegisterValueChangedCallback(e =>
			{
				node.title = e.newValue;
				node.DialogueText= e.newValue;
			});
			node.contentContainer.Add(tf);

			node.RefreshExpandedState();
			node.RefreshPorts();
			node.SetPosition(new Rect(pos, NodeSize));

			return node;
		}

        public void AddChoicePort(DialogueNode node,string name = "",Direction direction = Direction.Output)
        {
            var generatedPort = GeneratePort(node, direction);
			var portName = name==""?"text":name;

			var textfield = new TextField()
			{
				name = string.Empty,
				value = portName,
			};
			textfield.RegisterValueChangedCallback(evt => generatedPort.portName = evt.newValue);
			textfield.styleSheets.Add(Resources.Load<StyleSheet>("DialogueGraph"));
			generatedPort.contentContainer.Add(new Label(" "));
			generatedPort.contentContainer.Add(textfield);
			var deleteButton = new Button(() => { RemovePort(node, generatedPort); })
			{
				text = "X"
			};

			generatedPort.contentContainer.Add(deleteButton);
            generatedPort.portName = name;
            node.outputContainer.Add(generatedPort);
			node.RefreshExpandedState();
            node.RefreshPorts();
        }

        private void RemovePort(DialogueNode node, Port generatedPort)
        {
			//�ҳ��ö˿������ӵ�����
			var targetEdge = edges.ToList().Where(x =>
				x.output.portName == generatedPort.portName && x.output.node == generatedPort.node
			);

			if (targetEdge.Any())
			{
				var edge = targetEdge.First();
				edge.input.Disconnect(edge);
				RemoveElement(targetEdge.First());
			}
			node.outputContainer.Remove(generatedPort);
			node.RefreshPorts();
			node.RefreshExpandedState();

        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
			var CompatiblePorts = new List<Port>();
			ports.ForEach(port =>
			{
				if(startPort!= port && startPort.node!=port.node)
					CompatiblePorts.Add(port);
			});
			return CompatiblePorts;
        }
    }
}
