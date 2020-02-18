using System.Linq;
using System.Collections;

using System.Collections.Generic;
using UnityEngine;

public class NavigatorBehaviour : MonoBehaviour
{
    /**
     * <summary>
     * Game objects that the navigator will be mapped upon.
     * </summary>
     */
    public List<GameObject> navigations;

    /**
     * <summary>
     * A list of nodes that contains the position of the navigation
     * </summary>
     */
    private List<NavigationNode> nodes;

    // Start is called before the first frame update
    private void Start()
    {
        //StartCoroutine(this.MapByVertices());
        StartCoroutine(this.MapByRaycastColliders());
    }

    // Update is called once per frame
    private void Update()
    {
        
    }

    private void MapNavigations()
    {

    }

    /**
     * <summary>
     * Get top vertexes
     * </summary>
     *
     * <returns>
     * List<Vector3>
     * </returns>
     */
    private IEnumerator GetTopVertices()
    {
        Transform transform = this.navigations[0].transform;
        Mesh mesh = transform.GetComponent<MeshFilter>().mesh;

        Renderer renderer = transform.GetComponent<Renderer>();

        Vector3 startingPoint = new Vector3(
            renderer.bounds.min.x,
            renderer.bounds.max.y,
            renderer.bounds.min.z
        );

        Debug.Log(startingPoint);

        float distance = 0.3f;

        Vector3 nextStartingPoint = new Vector3(
            startingPoint.x,
            startingPoint.y,
            startingPoint.z
        );

        bool goNextZ = true;

        while (true)
        {
            if (!renderer.bounds.Contains(nextStartingPoint))
            {
                yield break;
            }

            Vector3 nodePoint = new Vector3(
                nextStartingPoint.x,
                nextStartingPoint.y,
                nextStartingPoint.z
            );

            Debug.Log(nodePoint);

            while (goNextZ)
            {
                Vector3 tempNextPointZNode = new Vector3(
                    nodePoint.x,
                    nodePoint.y,
                    nodePoint.z + distance
                );

                if (!renderer.bounds.Contains(tempNextPointZNode))
                {
                    goNextZ = false;
                    yield return null;
                }
                
                this.CreateNodePoints(nodePoint);
                nodePoint.z += distance;

                yield return null;
            }

            nextStartingPoint.x += distance;

            goNextZ = true;

            yield return null;
        }

        //List<Vector3> vertices = new List<Vector3>();
        //foreach (Vector3 vertice in mesh.vertices)
        //{
        //    vertices.Add(this.navigations[0].transform.TransformPoint(vertice));
        //}

        //vertices = vertices.OfType<Vector3>().ToList();
        //List<Vector3> topVertices = vertices.Where(vertice => vertice.y > 0).ToList();

        //this.CreateNodePoints(vertices);

        //return new List<Vector3>();
    }

    /**
     * <summary>
     * Create a node point based on the given point locations
     * </summary>
     *
     * <param name="points"></param>
     * <returns>
     * void
     * </returns>
     */
    public void CreateNodePoints(List<Vector3> points)
    {
        foreach (Vector3 point in points)
        {
            GameObject nodePoint = MonoBehaviour.Instantiate(Resources.Load("Prefabs/Node Point")) as GameObject;
            nodePoint.transform.position = point;

            Debug.Log(point);
        }
    }

    /**
     * <summary>
     * Create a node point based on the given point location
     * </summary>
     *
     * <param name="point"></param>
     * <returns>
     * void
     * </returns>
     */
    public void CreateNodePoints(Vector3 point)
    {
        GameObject nodePoint = MonoBehaviour.Instantiate(Resources.Load("Prefabs/Node Point")) as GameObject;
        nodePoint.transform.position = point;

        Debug.Log(point);
    }

    public IEnumerator MapByVertices()
    {
        Transform transform = this.navigations[0].transform;
        Mesh mesh = transform.GetComponent<MeshFilter>().mesh;

        // Modify z value to 2 precisions
        List<Vector3> vertices = new List<Vector3>();

        for (int i = 0; i < mesh.vertices.Length; i++)
        {
            Vector3 vertice = mesh.vertices[i];

            Vector3 newVertice = new Vector3(
                vertice.x,
                vertice.y,
                vertice.z
            );

            vertices.Add(newVertice);
        }

        vertices = vertices.Where((vertice) => { return transform.TransformPoint(vertice).y > 0f; }).Distinct().ToList();

        List<Vector3> sortedVertices = vertices.OrderBy(vertice => vertice.z).ToList();

        // Grouping
        List<List<Vector3>> group = new List<List<Vector3>>();
        int groupIndex = -1;

        Vector3 prevVertice = Vector3.zero;

        for (int i = 0; i < sortedVertices.Count(); i++)
        {
            Vector3 vertice = sortedVertices[i];

            bool sameZCoord = prevVertice != Vector3.zero && Mathf.Approximately(prevVertice.z, vertice.z);
            if (sameZCoord)
            {
                // Append to the last group
                List<Vector3> zGroup = group[groupIndex];
                zGroup.Add(vertice);
            }
            else
            {
                // Create new list group
                List<Vector3> zGroup = new List<Vector3>();
                zGroup.Add(vertice);

                group.Add(zGroup);
                groupIndex++;
            }

            prevVertice = vertice;

            yield return null;
        }

        foreach (List<Vector3> list in group)
        {
            for (int n = 0; n < list.Count() - 1; n++)
            {
                Vector3 vertice = transform.TransformPoint(list[n]);
                Vector3 nextVertice = transform.TransformPoint(list[n + 1]);

                float slope = (nextVertice.y - vertice.y) / (nextVertice.x - vertice.x);
                float distance = 1f;

                for (float x = vertice.x; (vertice.x + distance) < nextVertice.x; x += distance)
                {
                    float xPos = x;
                    float yPos = slope * xPos + vertice.y;
                    float zPos = vertice.z;

                    Vector3 point = new Vector3(x, yPos, zPos);
                    this.CreateNodePoints(point);

                    yield return null;
                }
            }

            yield return null;
        }
    }

    // TODO: Map 3D navigations?, with multiple objects
    // Connect the nodes to make the A* work properly.
    public IEnumerator MapByColliders()
    {
        Transform transform = this.navigations[0].transform;
        Mesh mesh = transform.GetComponent<MeshFilter>().mesh;

        Collider renderer = transform.GetComponent<Collider>();

        Vector3 startingPoint = new Vector3(
            renderer.bounds.min.x,
            renderer.bounds.min.y,
            renderer.bounds.min.z
        );

        Vector3 nodePoint = new Vector3(
            startingPoint.x,
            startingPoint.y,
            startingPoint.z
        );

        float distance = 0.4f;

        float verticalExtents = 1f;
        for (float x = startingPoint.x; x < renderer.bounds.max.x; x += distance)
        {
            for (float z = startingPoint.z; z < renderer.bounds.max.z; z += distance)
            {
                for (float y = startingPoint.y; y < renderer.bounds.max.y + verticalExtents; y += distance) {
                    Vector3 point = new Vector3(x, y, z);

                    ExtensionDebugger.DrawBox(point, Vector3.one / 16, Quaternion.identity, Color.red);
                    Collider[] colliders = Physics.OverlapBox(point, Vector3.one / 16, Quaternion.identity);

                    if (colliders.Length > 0)
                    {
                        this.CreateNodePoints(point);
                    }

                    yield return null;
                }

                //yield return null;
            }

            //yield return null;
        }
    }

    // TODO: Map 3D navigations?, with multiple objects
    // Connect the nodes to make the A* work properly.
    public IEnumerator MapByRaycastColliders()
    {
        // Get all vertices into a list
        // Make sure it is in world point not local
        List<Vector3> vertices = this.GetWorldPointVertices();

        float left = vertices.Min(vertice => vertice.x);
        float top = vertices.Max(vertice => vertice.y);
        float bottom = vertices.Min(vertice => vertice.y);
        float right = vertices.Max(vertice => vertice.x);
        float inner = vertices.Max(vertice => vertice.z);
        float outter = vertices.Min(vertice => vertice.z);

        Vector3 startingPoint = new Vector3(left, bottom, outter);

        float distance = 1f;
        for (float x = startingPoint.x; x < right; x += distance)
        {
            for (float z = startingPoint.z; z < inner; z += distance)
            {
                float yOffset = 0.5f;
                Vector3 point = new Vector3(x, top + yOffset, z);

                float rayLength = top - bottom;

                Vector3 lineEnd = new Vector3(point.x, bottom, point.z);

                Debug.DrawLine(point, lineEnd, Color.red, Mathf.Infinity);
                RaycastHit[] hits = Physics.RaycastAll(point, Vector3.down, rayLength);

                foreach (RaycastHit hit in hits)
                {
                    // Test: subject to change for production or reliability.
                    if (hit.transform.name == "Cube")
                    {
                        this.CreateNodePoints(hit.point);
                    }
                }
            }
            yield return null;
        }
    }

    /**
     * <summary>
     * Get all vertices in the navigations, convert it to the world points.
     * Local vertice point to world vertice point
     * </summary>
     *
     * <returns>
     * List<Vector3> vertices
     * </returns>
     */
    public List<Vector3> GetWorldPointVertices()
    {
        List<Vector3> vertices = new List<Vector3>();

        foreach (GameObject navigation in this.navigations)
        {
            Transform transform = navigation.transform;
            Mesh mesh = transform.GetComponent<MeshFilter>().mesh;

            foreach (Vector3 vertice in mesh.vertices)
            {
                vertices.Add(transform.TransformPoint(vertice));
            }
        }

        return vertices;
    }
}