using UnityEngine;
using TMPro;
using System.Collections;

public class NarrativeTrigger : MonoBehaviour
{
    public TextMeshProUGUI thoughtText;

    private bool triggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !triggered)
        {
            triggered = true;
            StartCoroutine(NarrativeSequence());
        }
    }

    IEnumerator NarrativeSequence()
    {
        thoughtText.text = "Oh no... I think there's a fire.";

        yield return new WaitForSeconds(2f);

        thoughtText.text = "<color=red><size=60><b>GET TO THE ELEVATOR QUICK!</b></size></color>";

        yield return new WaitForSeconds(3f);

        thoughtText.text = "";
    }
}