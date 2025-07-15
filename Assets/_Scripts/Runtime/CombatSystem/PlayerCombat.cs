using System.Collections.Generic;
using System.Linq;
using ProjectEmbersteel.Equipment;
using UnityEngine;

namespace ProjectEmbersteel
{
    public class PlayerCombat : MonoBehaviour
    {
        //[SerializeField] private VoidReturnDoubleWeaponSOParameterEventChannelSO _weaponChangedEventChannelSO; // Event channel to notify when the weapon changes.
        [Space]
        [SerializeField] private float _attackRate = 2f;

        private Animator _animator;
        private List<DamageDealer> _damageDealer;
        private float _nextAttackTime = 0f;

        private void Awake() => _animator = GetComponent<Animator>();

        private void OnEnable()
        {
            //_weaponChangedEventChannelSO.OnEventRaised += OnWeaponChanged;
        }

        private void OnDisable()
        {
            //_weaponChangedEventChannelSO.OnEventRaised -= OnWeaponChanged;
        }

        // Uncomment and modify this if you plan to use it for handling attack input.
        //private void Update()
        //{
        //    if (Time.time >= _nextAttackTime)
        //    {
        //        if (Input.GetButtonDown("Fire2"))
        //        {
        //            _animator.SetTrigger("Attack");
        //            _nextAttackTime = Time.time + 1f / _attackRate;
        //        }
        //    }
        //}

        public void StartDealingDamage() => _damageDealer?.ForEach(x => x.ApplyDamage());

        public void StopDealingDamage() => _damageDealer?.ForEach(x => x.EndDamage());

        private void OnWeaponChanged(WeaponSO newWeapon, WeaponSO oldWeapon)
        {
            if (newWeapon != null)
            {
                _damageDealer = GetComponentsInChildren<DamageDealer>().ToList();
            }
            else
            {
                _damageDealer = null;
            }
        }
    }
}