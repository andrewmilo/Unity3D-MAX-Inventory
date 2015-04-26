using UnityEngine;
using System.Collections;

/// <summary>
/// Interface for the ability to control object's cooldown state.
/// NOTE: StartCooldown should ideally be called through a Monobehaviour Coroutine, ex. (StartCoroutine(StartCooldown(5))).
/// </summary>
public interface ICooldown {
	bool OnCooldown {get;set;}
	IEnumerator StartCooldown(float time);
	CooldownManager cooldownGO {get;set;}
}
