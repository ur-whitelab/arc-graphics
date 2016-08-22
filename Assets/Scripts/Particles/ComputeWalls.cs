using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class ComputeWalls : Compute
{
    private int wallHandle;
    public ComputeShader wallShader;

    private ComputeBuffer walls;
    private List<ShaderConstants.Wall> cpu_walls = new List<ShaderConstants.Wall>();

    public override void SetupShader(ParticleManager pm)
    {
        wallHandle = wallShader.FindKernel("Walls");

        if (cpu_walls.Count != 0)
        {
            walls.SetData(cpu_walls.ToArray());
        }


        wallShader.SetBuffer(wallHandle, "positions", pm.positions);
        wallShader.SetBuffer(wallHandle, "velocities", pm.velocities);
        wallShader.SetBuffer(wallHandle, "lastPositions", pm.lastPositions);
        wallShader.SetBuffer(wallHandle, "forces", pm.forces);
        wallShader.SetBuffer(wallHandle, "properties", pm.properties);
        wallShader.SetFloat("timeStep", pm.TimeStep);        

    }

    public void AddWall(Vector2[] points)
    {
        AddWall(points);
    }


    public void AddWall(Vector3[] points)
    {
        //need to add zero norm, to indicate that the this one is not in use (it's a gap)        
        cpu_walls.Add(new ShaderConstants.Wall(points[0], Vector2.zero));
        for(int i = 1; i < points.Length; i++)
        {
            //claculate a normal vector for use in geometry algorithms
            //in 3D it's the cross of the two vectors that define the plane
            //in 2D, it's what is below.
            Vector2 norm = (points[i] - points[i - 1]);
            if (norm.y == 0)
                norm = new Vector2(0, 1);
            else
                norm = new Vector2(1, -norm.x / norm.y);
            norm.Normalize();

            cpu_walls.Add(new ShaderConstants.Wall(points[i], norm));
        }
        if(walls != null)
            walls.Release();
        walls = new ComputeBuffer(cpu_walls.Count, ShaderConstants.WALL_STRIDE);
        walls.SetData(cpu_walls.ToArray());
        wallShader.SetBuffer(wallHandle, "walls", walls);
    }

    public override void UpdatePostIntegrate(int nx)
    {
        if(cpu_walls.Count > 0)
            wallShader.Dispatch(wallHandle, nx, 1, 1);
    }
}
