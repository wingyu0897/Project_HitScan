using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Health : NetworkBehaviour
{
    public int MaxHealth = 100;
    public NetworkVariable<int> CurrentHealth;

	public override void OnNetworkSpawn()
	{
	}

	public override void OnNetworkDespawn()
	{
	}
}
