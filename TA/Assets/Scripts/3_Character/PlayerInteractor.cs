using System.Collections.Generic;
using UnityEngine;

public class PlayerInteractor : MonoBehaviour
{
    readonly List<IInteractable> interactables = new List<IInteractable>();

    public void Register(IInteractable interactable)
    {
        if (interactable == null || interactables.Contains(interactable))
            return;

        interactables.Add(interactable);
    }

    public void Unregister(IInteractable interactable)
    {
        if (interactable == null)
            return;

        interactables.Remove(interactable);
    }

    public bool TryInteract(Player player)
    {
        IInteractable target = GetClosestInteractable();
        if (target == null)
            return false;

        target.Interact(player);
        return true;
    }

    IInteractable GetClosestInteractable()
    {
        IInteractable closest = null;
        float closestDistance = float.MaxValue;

        for (int i = interactables.Count - 1; i >= 0; i--)
        {
            IInteractable interactable = interactables[i];
            Component component = interactable as Component;
            if (component == null)
            {
                interactables.RemoveAt(i);
                continue;
            }

            float distance = (component.transform.position - transform.position).sqrMagnitude;
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = interactable;
            }
        }

        return closest;
    }
}
