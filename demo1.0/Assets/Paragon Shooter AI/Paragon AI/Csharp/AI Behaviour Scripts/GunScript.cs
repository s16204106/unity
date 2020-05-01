﻿using UnityEngine;
using System.Collections;

/*
 * Manages the firing of the gun, and has minor influence on aiming.
 * Also deals with secondary fire
 * */

namespace ParagonAI{
public class GunScript : MonoBehaviour {

	//Stuff
	public ParagonAI.BaseScript myAIBaseScript;
	public ParagonAI.AnimationScript animationScript;
    public AudioSource audioSource;

	int[] enemyTeams;	

	//Bullet stuff	
	public GameObject bulletObject;
	public AudioClip bulletSound;
	[Range (0.0f, 1.0f)]
	public float bulletSoundVolume = 1;	
	public Transform  bulletSpawn;	
	public GameObject muzzleFlash;
	public Transform muzzleFlashSpawn;
	public float flashDestroyTime = 0.3f;
	bool canCurrentlyFire = true;

	
	//Used for shotguns; 1 is default and should be used for non-shotgiun weapons
	public int pelletsPerShot = 1;	
	
	//Just to determine whether or not or projectiles should home in on our enemies
	public bool isRocketLauncher = false;
	
	//Secondary Fire
	public GameObject secondaryFireObject;
	//1 = highest probability
	[Range (0.0f, 1.0f)]
	public float oddsToSecondaryFire = 0.1f;
	public float minDistForSecondaryFire = 10;
	public float maxDistForSecondaryFire  = 50;
	bool canFireGrenadeAgain = false;	
	Vector3 lastPosTargetSeen = Vector3.zero;
	public bool needsLOSForSecondaryFire = false;
	bool canThrowGrenade = true;
	public  float minTimeBetweenSecondaryFire = 4;	

	
	//RoF, burst and timer Stuff		
	public float minPauseTime = 1;
	public float randomPauseTimeAdd  = 2;	
	public int minRoundsPerVolley = 1;
	public int maxRoundsPerVolley = 10;
	public int minBurstsPerVolley;
	public int maxBurstsPerVolley;	
	public int currentRoundsPerVolley;	
	public float rateOfFire = 2;
	float timeBetweenBursts;	
	public float burstFireRate = 12;
	public int shotsPerBurst = 1;
	float timeBetweenBurstBullets;
	
	//Reloading
	public int bulletsUntilReload = 60;
	public AudioClip reloadSound;	
	[Range (0.0f, 1.0f)]
	public float reloadSoundVolume = 1;
	//bool isReloading = false;
	int currentBulletsUntilReload = 0;
	public float reloadTime = 2;	

	//Accuracy
	public float inaccuracy = 1;	
	[Range (0.0f, 90.0f)]
	public float maxFiringAngle = 10;	
	[Range (0.0f, 90.0f)]
	public float maxSecondaryFireAngle = 40;
	Quaternion fireRotation;	
	
	//Transforms
	Transform targetTransform;
	Transform LOSTargetTransform;
	
	//LoS stuff
	LayerMask LOSLayermask;
	public float timeBetweenLOSChecks = 2;
	//bool canSeePlayer = true;
	//bool canDoLOSCheck = false;
	
	//Private status stuff
	bool aware = false;
	bool isFiring = false;
	bool isWaiting = false;
	
	//Cover	
	public float distInFrontOfTargetAllowedForCover = 3;
	public float coverTransitionTime = 0.4f;
	float rayDist;

	//Sound
	public float soundRadius = 7;	

	
	void Awake (){
		//Set default values, calculate squared distances, etc.
		LOSLayermask = ParagonAI.ControllerScript.currentController.GetLayerMask();

        if (!audioSource)
            if (bulletSpawn.gameObject.GetComponent<AudioSource>())
                audioSource = bulletSpawn.gameObject.GetComponent<AudioSource>();

		isFiring = false;
		isWaiting= false;
		currentBulletsUntilReload = bulletsUntilReload;	
		timeBetweenBurstBullets = 1/burstFireRate;
		timeBetweenBursts = 1/rateOfFire;	
		minBurstsPerVolley = (int)(minRoundsPerVolley/shotsPerBurst);
		maxBurstsPerVolley = (int)(maxRoundsPerVolley/shotsPerBurst);	
		maxFiringAngle /= 2;
		maxSecondaryFireAngle /= 2;
	}
		
	// Stuff we need done after all other stuff is set up
	void Start () {
		enemyTeams = myAIBaseScript.GetEnemyTeamIDs();
	}
	
	
	// Update is called once per frame
	void LateUpdate () {
				if(aware)
					{
                        //If we're not doing anythingm start the bullet firing cycle
						if(!isFiring && !isWaiting && bulletObject)
							{						
								StartCoroutine(BulletFiringCycle());
							}	
						else if(!bulletObject)	
							{
								Debug.LogWarning("Can't fire because there is no bullet object selected!");
							}
					}	
	}
	
	//Shooting////////////////////////////////////////////////////////	
	IEnumerator BulletFiringCycle()
	{

		//Fire
		isFiring = true;
		
        //Wait for the animation transitioning the agent from hiding to a firing positiont o finish.
		if(myAIBaseScript.inCover)
			yield return new WaitForSeconds(coverTransitionTime);

        //Don't fire if the agent is unaware of the target or meleeing the target.
        if (myAIBaseScript.IsEnaging() && !myAIBaseScript.isMeleeing)
			{
                //If we have clear LoS to the LOSTargetTransform, fire
                //You may want to check for line of sight to a position right above the target's head (for example)
                //This will allow your agent to lay down suppressing fire even if they can't see the target.
				if(LOSTargetTransform)
					{
                        //See if we can use our secondary fire
                        //While a grenade may not need LoS, a homing missile might
						if(!Physics.Linecast(bulletSpawn.position, LOSTargetTransform.position, LOSLayermask))	
							{
								lastPosTargetSeen = targetTransform.position;
								canFireGrenadeAgain = true;							
								FireOneGrenade();
								canFireGrenadeAgain = true;										
							}
						else if(!needsLOSForSecondaryFire)
							{			
								if(canFireGrenadeAgain)
									FireOneGrenade();
                                canFireGrenadeAgain = true;	
							}
					}
                //Create the sound that will be heard by Paragon AI agents
                //This sound is not going to be heard by the player
				if(soundRadius > 0)
					{
						ParagonAI.ControllerScript.currentController.CreateSound(bulletSpawn.position, soundRadius, enemyTeams);
					}
				//Shoot regular bullets	
				yield return StartCoroutine(Fire());
			}
		
		//Transition
		isWaiting = true;
		isFiring = false;
	
					
        //If we aren't reloading wait for a while before firing another burst
		if(currentBulletsUntilReload > 0 && reloadTime > 0)
			{
				yield return new WaitForSeconds(minPauseTime + Random.value * randomPauseTimeAdd);
			}
		else
			{
                //If we're out of ammo, reload.
				if(reloadSound)
					{
                        audioSource.volume = reloadSoundVolume;
                        audioSource.PlayOneShot(reloadSound);
					}
				if(animationScript)	
					{
						animationScript.PlayReloadAnimation();
					}
				yield return new WaitForSeconds(reloadTime);
				currentBulletsUntilReload = bulletsUntilReload;
				yield return new WaitForSeconds(minPauseTime * Random.value);
			}
		isWaiting= false;
	}
	
	IEnumerator Fire()
	{
        //Make sure we don't fire more bullets than the number remaining in the agent's magazine.
		currentRoundsPerVolley = Mathf.Min(Random.Range(minBurstsPerVolley, maxBurstsPerVolley), currentBulletsUntilReload);
	
        //Make sure we have bullets left and stop firing if the agent is dead.
		while(currentRoundsPerVolley > 0 && this.enabled)
			{	
				if(LOSTargetTransform && canCurrentlyFire)
					{		
						//Make sure ray is always facing "forward".
                        //Make sure we have clear LoS to the target
                        //Ray can be stopped short so that the agent will still fire at the target even if they are behind a thin layer of cover
						rayDist	= Mathf.Max(0.00001f, Vector3.Distance(bulletSpawn.position, LOSTargetTransform.position) - distInFrontOfTargetAllowedForCover);		
						if(rayDist == 0 || !Physics.Raycast(bulletSpawn.position, LOSTargetTransform.position-bulletSpawn.position, rayDist, LOSLayermask))	
							{	
                                //Fire a burst of a fixed number of bullets.  Usually this number is one.
								for(int i = 0; i < shotsPerBurst; i++)
									{
										if(i < shotsPerBurst-1)	
											yield return new WaitForSeconds (timeBetweenBurstBullets);	
										currentBulletsUntilReload--;		
										FireOneShot();
									}
							}															
	
						currentRoundsPerVolley--;
					}
				else
					{
						yield break;
					}
                if (currentRoundsPerVolley > 0)
                {
				    yield return new WaitForSeconds(timeBetweenBursts);
                }
			}
	}
		
	void FireOneShot()
	{
		//Look At Target
		if(targetTransform)
			{	
                //Snap our aim to the target even if we're aiming slightly off
                //This is because the RotateToAimGunScript will rarely point directly at the target- merely close enough
			    bool amAtTarget = Vector3.Angle(bulletSpawn.forward, targetTransform.position - bulletSpawn.position) < maxFiringAngle;
				
				//Fire Shot
                //Most bullets will have one bullet.  However, shotguns and similar weapons will have more.
				for(int j = 0; j < pelletsPerShot; j++)
					{
                        if (amAtTarget)
                        {
                            fireRotation.SetLookRotation(targetTransform.position - bulletSpawn.position);
                        }
                        else
                        {
                            fireRotation = Quaternion.LookRotation(bulletSpawn.forward);
                        }

                        //Modify our aim by a random amound to simulate inaccuracy.
                        fireRotation *= Quaternion.Euler(Random.Range(-inaccuracy, inaccuracy), Random.Range(-inaccuracy, inaccuracy), 0); 

						GameObject bullet = (GameObject)(Instantiate(bulletObject, bulletSpawn.position, fireRotation));
                        //If this is using the ParagonAI Bullet Script and is a rocket launcher
						if(isRocketLauncher && bullet.GetComponent<ParagonAI.BulletScript>())
							{
								bullet.GetComponent<ParagonAI.BulletScript>().SetAsHoming(targetTransform);
							}
					}
				
                //Play the sound that is audible by the player
				if(bulletSound)
					{
                        audioSource.volume = bulletSoundVolume;
                        audioSource.PlayOneShot(bulletSound);
					}
					
				if(animationScript)	
					{
						animationScript.PlayFiringAnimation();
					}				
				
                //Createthe muzzle flash and then destroy it after a given amount of time
				if(muzzleFlash)
					{
						GameObject flash = (GameObject)(Instantiate(muzzleFlash, muzzleFlashSpawn.position, muzzleFlashSpawn.rotation));
						flash.transform.parent = muzzleFlashSpawn;
						GameObject.Destroy(flash, flashDestroyTime);	
					}
			}
	}
	
	
	//Shooting////////////////////////////////////////////////////////
	//Secondary Fire////////////////////////////////////////////////////////
	void FireOneGrenade()
	{
		if(Random.value < oddsToSecondaryFire && canThrowGrenade && Vector3.Angle(bulletSpawn.forward, lastPosTargetSeen - bulletSpawn.position)  < maxSecondaryFireAngle && secondaryFireObject)
			{ 
				float dist = Vector3.Distance(lastPosTargetSeen, bulletSpawn.position);
				if(dist < maxDistForSecondaryFire && dist > minDistForSecondaryFire)
					{
						canFireGrenadeAgain = false;	
						StartCoroutine(SetTimeUntilNextGrenade());
					
						//Fire Secondary Fire
					    GameObject currentGrenade = (GameObject)(Instantiate(secondaryFireObject, bulletSpawn.position, bulletSpawn.rotation));
					    
					    //Stuff to deal with Grenades
					    currentGrenade.transform.LookAt(lastPosTargetSeen);
					    if(currentGrenade.GetComponent<ParagonAI.GrenadeScript>())
					    	{
					    		currentGrenade.GetComponent<ParagonAI.GrenadeScript>().SetTarget(lastPosTargetSeen);
					    	}	
					   }			
			}			
	}
	//Secondary Fire////////////////////////////////////////////////////////

		
	//Setters
	public void EndEngage()
	{
		targetTransform = null;
		aware = false;
	}
	
	public void AssignTarget(Transform newTarget, Transform newLOSTarget){	
		targetTransform = newTarget;
		LOSTargetTransform = newLOSTarget;
		aware = true;			
	}	
	
	public void SetCanCurrentlyFire(bool b)
	{
		canCurrentlyFire = b;
	}
	
	public void SetAware()
	{
		aware = true;
	}
	
	public void SetEnemyBaseScript(ParagonAI.BaseScript x)
	{
		myAIBaseScript = x;
	}	
	
	public void ChangeTargets(Transform newl,Transform newt)
	{
		LOSTargetTransform = newl;
		targetTransform = newt;				
	}	
	
	IEnumerator SetTimeUntilNextGrenade()
	{
		canThrowGrenade = false;
		yield return new WaitForSeconds(minTimeBetweenSecondaryFire);
		canThrowGrenade = true;
	}		

	public bool IsFiring()
	{
		return isFiring;
	}		
}
}