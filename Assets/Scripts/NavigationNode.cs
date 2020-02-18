using System;
using UnityEngine;

public class NavigationNode
{
    /**
     * <summary>
     * Position
     * </summary>
     */
    private Vector3 position;

    /**
     * <summary>
     * Constructor
     * </summary>
     */
    public NavigationNode (Vector3 position)
    {
        this.SetPosition(position);
    }

    /**
     * <summary>
     * Set position
     * </summary>
     *
     * <param>Vector3 position</param>
     */
    public void SetPosition (Vector3 position)
    {
        this.position = position;
    }

    /**
     * <summary>
     * Get world point position
     * </summary>
     *
     * <returns>
     * Vector3 this.position
     * </returns>
     */
    public Vector3 GetPosition ()
    {
        return this.position;
    }
}
