using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class UI_Agent_Display : MonoBehaviour
{
    public IAgent assignedAgent;
    public UIElement movementArrow;
    public UIElement agentOutline;
    [System.Serializable]
    public class UIElement
    {
        public RectTransform parentRectTransform;
        public RectTransform rectTransform;
        public Image image;
    }
    public void AssignAgent(IAgent newAgent)
    {
        assignedAgent = newAgent;
    }
    private void Start()
    {
        HideUI();
    }
    public void Update()
    {
        if (assignedAgent == null)
            return;
        if(assignedAgent.args.isDead || !assignedAgent.args.isActive || !assignedAgent.args.hasInstruction)
        {
            assignedAgent = null;
            HideUI();
            return;
        }
        #region movementArrow
        if (assignedAgent.args.path != null)
        {
            movementArrow.image.color = AgentColor();
            Vector2 startPos = GameManager.ins.GetScreenPosition(assignedAgent.args.path.start);
            Vector2 endPos = GameManager.ins.GetScreenPosition(assignedAgent.args.path.end);
            float size = Vector2.Distance(startPos, endPos);
            movementArrow.parentRectTransform.position = startPos;
            movementArrow.rectTransform.transform.right = endPos - startPos;
            movementArrow.rectTransform.anchoredPosition = movementArrow.rectTransform.transform.right * (size / 2);
            //movementArrowRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 50, size);
        }
        if (assignedAgent.args.path == null || !assignedAgent.args.hasInstruction)
        {
            movementArrow.image.color = Color.clear;
        }
        #endregion
        #region agentOutline
        agentOutline.image.color = AgentColor();
        agentOutline.parentRectTransform.position = assignedAgent.args.screenPos;
        #endregion
    }
    void HideUI()
    {
        movementArrow.image.color = Color.clear;
        agentOutline.image.color = Color.clear;
    }
    Color AgentColor()
    {
        if (assignedAgent == null)
            return Color.clear;
        if (assignedAgent.args.type == AgentType.Player)
            return Color.blue * 0.85f;
        if (assignedAgent.args.type == AgentType.Defender)
            return Color.green*0.85f;
        if (assignedAgent.args.type == AgentType.Enemy)
            return Color.red*0.85f;
        return Color.clear;
    }
}
