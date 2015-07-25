using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SSTUTools
{
	//	Allows a part to add or remove copies of the nodes specified in its part.cfg file, as well as optionally inverting the direction of those nodes.
	//	Intended to be used by the LanderCore series of parts as they will require complex node setup to facilitate construction
	public class SSTUSelectableNodes : PartModule
	{		
		[KSPField]
		public String nodeName = "top";
		
		[KSPField]
		public bool canInvert = true;						
		
		[KSPField]
		public int numOfNodes = 4;
		
		[KSPField]
		public bool startWithOpenNode = true;
		
		//TODO
		[KSPField(isPersistant=true)]
		public int invertedBitfield = 0;
		
		List<AttachNode> nodes = new List<AttachNode>();			
										
		public SSTUSelectableNodes ()
		{

		}	
		
		public override void OnStart (PartModule.StartState state)
		{
			base.OnStart (state);							
			Events["removeNode"].guiName = "Remove Node - "+nodeName;
			Events["addNode"].guiName = "Add Node - "+nodeName;
			Events["invertNode"].guiName = "Invert Node - "+nodeName;
			if(!HighLogic.LoadedSceneIsEditor){return;}		//only active in editor		
			
			//remove all un-attached nodes
			bool leftOneNode = startWithOpenNode ? false : true;
			AttachNode node;
			int foundNodes = 0;
			for(int i = 0; i < numOfNodes; i++)
			{
				node = part.findAttachNode(nodeName + (i+1));
				if(node!=null){foundNodes++;}
				if(node.attachedPart==null)
				{
					if(leftOneNode)
					{
						removeNodeInternal(node);
					}
					else
					{
						leftOneNode = true;
					}
				}
				nodes.Add (node);
			}
			numOfNodes = foundNodes;
		}
		
		[KSPEvent (name= "addNode", guiName = "Add Node", guiActiveUnfocused = false, externalToEVAOnly = false, guiActive = false, guiActiveEditor = true)]
		public void addNode()
		{
			addNodeInternal();			
		}

		[KSPEvent (name= "removeNode", guiName = "Remove Node", guiActiveUnfocused = false, externalToEVAOnly = false, guiActive = false, guiActiveEditor = false)]
		public void removeNode()
		{
			AttachNode node;
			for(int i = 0; i < numOfNodes; i++)
			{
				node = part.findAttachNode(nodeName + (i+1));
				if(node.attachedPart==null)
				{
					removeNodeInternal(node);
					break;
				}
			}	
		}
		
		[KSPEvent (name= "invertNode", guiName = "Invert Node", guiActiveUnfocused = false, externalToEVAOnly = false, guiActive = false, guiActiveEditor = false)]
		public void invertNode()
		{
			AttachNode node;
			for(int i = 0; i < numOfNodes; i++)
			{
				node = part.findAttachNode(nodeName + (i+1));
				if(node.attachedPart==null)
				{
					node.orientation.x *=-1;
					node.orientation.y *=-1;
					node.orientation.z *=-1;
					break;
				}
			}
		}

		[KSPEvent (name= "debugSpew", guiName = "Debug Output", guiActive = false, guiActiveEditor = false)]
		public void debugSpew()
		{

			print ("Node config debug output for: "+numOfNodes+" total nodes.");
			AttachNode node;
			for(int i = 0; i < numOfNodes; i++)
			{
				node = nodes[i];
				if(node==null)
				{
					
				}
				else
				{
					print ("Found node for index: "+i+
					       "\nwith name of: "+node.id+
					       "\nhas attached part of: "+node.attachedPart+
					       "\nwith attached part ID: "+node.attachedPartId+
					       "\nat node orientation: "+node.orientation);					
				}
			}
			print("----------End Node config debug output-----------");
		}

				
		public void Update()
		{			
			if(!HighLogic.LoadedSceneIsEditor){return;}		//only active in editor
			
			bool foundEmpty = false;
						
			foreach(AttachNode node in nodes)
			{
				if(node.attachedPart==null && part.attachNodes.Contains(node))
				{
					if(foundEmpty)
					{
						removeNodeInternal(node);
					}
					foundEmpty = true;
				}	
			}
			
			if(foundEmpty)
			{
				Events["addNode"].guiActiveEditor = false;
				Events["removeNode"].guiActiveEditor = true;
				Events["invertNode"].guiActiveEditor = canInvert;
			}
			else
			{
				Events["addNode"].guiActiveEditor = true;
				Events["removeNode"].guiActiveEditor = false;
				Events["invertNode"].guiActiveEditor = false;
			}
		}
		
		private void removeNodeInternal(AttachNode node)
		{
			part.attachNodes.Remove(node);
			if(node.icon!=null){node.icon.SetActive(false);}
		}
		
		private void addNodeInternal()
		{			
			AttachNode node = null;
			
			for(int i = 0; i< nodes.Count; i++)
			{
				if(nodes[i].attachedPart==null)
				{
					node = nodes[i];
					break;
				}
			}
					
			if(node!=null)
			{
				if(node.icon!=null){node.icon.SetActive(true);}
				part.attachNodes.Add (node);				
			}
		}
	}
}
