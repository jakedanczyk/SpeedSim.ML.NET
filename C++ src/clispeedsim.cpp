#include "SpeedKernel.h"
#include <iostream>

bool Simulate(int count = 1);

void main(int argc, char* argv[]) {
	int count = atoi(argv[1]);
	Simulate(count);
	getchar();
}


/// simulate the battle
bool Simulate(int count)
{
	m_InitSim = false;

	m_DataIsDeleted = false;
	m_SimulateFreedItsData = false;

	if (count == 0)
		return false;

	int def_won = 0, att_won = 0, draw = 0;

	// init internal values, count ships etc.
	InitSim();
#ifdef CREATE_ADV_STATS
	m_DebrisFields.resize(count);
	m_LossAtt.resize(count);
	m_LossDef.resize(count);
	m_CombatResultsAtt.resize(count);
	m_CombatResultsDef.resize(count);
	for (int i = 0; i < count; i++)
	{
		m_CombatResultsAtt[i].resize(T_END);
		m_CombatResultsDef[i].resize(T_END);
	}

#endif

	vector<Obj> Att, Def;
	// backup set fleet
	// possibly copy into file for less memory usage when using _very_ big fleets?
	Att = (*m_AttObj);
	Def = (*m_DefObj);

	bool aborted = false;

	m_InitSim = true;
	int num, round;
	m_CurrentSim = 0;

	for (num = 0; num < count; num++)
	{
		m_CurrentSim++;
		(*m_AttObj) = Att;
		(*m_DefObj) = Def;
		for (round = 1; round < 7; round++)
		{
			if (m_DataIsDeleted)
			{
				aborted = true;
				break;
			}

			if (m_FuncPtr)
				m_FuncPtr(num + 1, round);

			// Save number of ships for combat report
			SaveShipsToCR(round);

			m_CurrentRound = round - 1;

			// maximize all shields
			MaxAllShields();
			// make every ship shoot
			size_t i;
			for (i = 0; i < m_AttObj->size(); i++)
			{
				ShipShoots((*m_AttObj)[i], ATTER, (*m_AttObj)[i].PlayerID);
			}

			for (i = 0; i < m_DefObj->size(); i++)
			{
				ShipShoots((*m_DefObj)[i], DEFFER, (*m_DefObj)[i].PlayerID);
			}

			// remove destroyed ships, calculate loss and debris
			DestroyExplodedShips();

			// battle ends
			if (round == 6 || m_AttObj->size() == 0 || m_DefObj->size() == 0)
			{
				SaveShipsToCR(round + 1);

				for (i = 0; i < m_AttObj->size(); i++)
				{
					ITEM_TYPE it = (*m_AttObj)[i].Type;
					DWORD Index = ((*m_AttObj)[i].PlayerID*T_RAK) + it;
					m_NumShipsAtt[Index].Num++;
				}

				for (i = 0; i < m_DefObj->size(); i++)
				{
					ITEM_TYPE it = (*m_DefObj)[i].Type;
					DWORD Index = ((*m_DefObj)[i].PlayerID*(T_END)) + it;
					m_NumShipsDef[Index].Num++;
				}

				// => attacker won
				if (m_AttObj->size() > 0 && m_DefObj->size() == 0)
					m_Result.AttWon++;
				// => defender won
				else if (m_DefObj->size() > 0 && m_AttObj->size() == 0)
					m_Result.DefWon++;
				else
					// => draw
					m_Result.Draw++;

				// recalculate best/worst case (check for better/worse case)
				UpdateBestWorstCase(num);

				break;
			}
		}
		if (aborted)
		{
			break;
		}
		m_Result.NumRounds += (round > 6 ? 6 : round);
	}
	Att.clear();
	Def.clear();

	m_CurrentSim = 0;

	if (!num && aborted)
		return false;

	else if (aborted)
	{
#ifdef CREATE_ADV_STATS
		// trim unused data
		m_DebrisFields.resize(num);
		m_LossAtt.resize(num);
		m_LossDef.resize(num);
		m_CombatResultsAtt.resize(num);
		m_CombatResultsDef.resize(num);
#endif
	}

	// battle result is added permanently added during multiple simulation
	// -> calculate average result by dividing through number of simulations
	m_Result /= num;
	size_t t;
	for (t = 0; t < m_NumShipsAtt.size(); t++)
	{
		m_NumShipsAtt[t].Num /= num;
	}
	for (t = 0; t < m_NumShipsDef.size(); t++)
	{
		m_NumShipsDef[t].Num /= num;
	}

	// now get the average number of ships in every round for combat reports
	ComputeCRArrays();

	// calculation of combat report
	ComputeBattleResult();

	m_SimulateFreedItsData = true;
#ifdef PROFILING
	ofstream aus("prof.txt");
	CProfiler::GetInst().GetResults(aus);
#endif
	return true;
}
