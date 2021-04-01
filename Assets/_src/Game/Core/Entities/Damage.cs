using System;
using System.Collections.Generic;

namespace Game.Entities
{
    public interface IDamage
    {
        float Value { get; }
    }

    public interface IDamaged
    {
        /// <summary>
        /// ���������� �����.
        /// ������ ���� ����� �����, ������� ����� ���������.
        /// ���������, ����� ���������� ����������, ���������� �����.
        /// </summary>
        IReadOnlyCollection<IDamage> Absorb { get; }

        /// <summary>
        /// ������������� �����.
        /// ������ ���� ����� �����, � ������� ���� ��������������.
        /// ���������, ����� ���������� �������������, ������ ����.
        /// </summary>
        IReadOnlyCollection<IDamage> Resist { get; }

        /// <summary>
        /// ���� ������� ���������� �������.
        /// �������� ������ ���� ��������� � ������������ � Absorb � Resist
        /// </summary>
        /// <param name="value"></param>
        void AddDamage(IUnit sender, float value);
    }
}