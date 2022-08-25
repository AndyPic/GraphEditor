using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI dialogueOutput;
    [SerializeField] private GameObject optionPrefab;

    [SerializeField] private GraphNodeStorage dialogue;

    private NodeData currentNode;

    private void OnEnable()
    {
        currentNode = dialogue.StartNode;
    }

    public void SelectOption(int optionIndex)
    { 
        if (optionIndex < 0 || currentNode.OutputPorts.Length <= optionIndex)
        {
            Debug.LogError(new IndexOutOfRangeException());
            return;
        }

        // Invoke the unity event for that option
        //currentNode.OutputPorts[optionIndex].onSelectOption.Invoke();

        // Advance current node
        currentNode = dialogue[currentNode.OutputPorts[optionIndex].ConnectedGUID];
    }
}