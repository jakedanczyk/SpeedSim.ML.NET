#include "SpeedKernel.h"
#include <iostream>
#include <string>
#include <fstream>

bool Simulate(int count = 1);
bool InitSim();
void ComputeShipData();
void MaxAllShields();
void EndRound();
void ShipsDontExplode();
void DestroyExplodedShips();
void ShipShoots(Obj& o, int Team, DWORD AtterID);
#ifdef INCL_OPTIMIZED_FUNCTIONS
void OptimizedShipShoots(Obj& o, int Team);
#endif

bool ParseSpioLine(genstring& s, TargetInfo& ti);
bool ReadCRTable(genstring Table, vector<SItem> &Fleet, ShipTechs &techs);
void ComputeLoot();
void ComputeBattleResult();
void UpdateBestWorstCase(int CurSim);
void ComputeLosses();
#ifdef INCL_OPTIMIZED_FUNCTIONS
void OptimizedComputeLosses();
#endif
#ifdef CREATE_ADV_STATS
void CreateAdvShipStats();
#endif

bool SetFleet(vector<SItem>* Attacker, vector<SItem>* Defender);

void SaveShipsToCR(int round);
void ComputeCRArrays();
void GenerateCRTable(genstrstream &out, const vector<SItem> &Items, int Team, int Player, TCHAR* Title);

int GetDistance(const PlaniPos& b, const PlaniPos& e);
DWORD ComputeFlyTime(const PlaniPos& b, const PlaniPos& e, int FleetID, const vector<SItem>& vFleet);
int GetShipSpeed(ITEM_TYPE Ship, int FleetID);
int GetFleetSpeed(int FleetID, const vector<SItem>& vFleet);

//genstring ReadStringFromIniFile(char *inifile, const TCHAR *Section, const TCHAR *KeyName);
//bool ReadLine(FILE *file, TCHAR *out, int max_length);
Res StringToRes(const genstring &val);

void InitRand();
ULONG RandomNumber(ULONG Max);
TCHAR* AddPointsToNumber(__int64 value, TCHAR* out);

Obj FillObj(ITEM_TYPE Type, int Team, DWORD PlayerID);
static bool ItemCompare(const SItem& a, const SItem& b);

bool CanShootAgain_FromTable(ITEM_TYPE AttType, ITEM_TYPE TargetType);
void FillRFTable(RFTYPE rfType);

void SetParseOrder();

// pointer to function which outputs number of rounds and simulations
void(*m_FuncPtr)(int sim, int round);

vector<SItem> m_NumShipsAtt;
vector<SItem> m_NumShipsDef;

vector<SItem> m_NumSetShipsAtt;
vector<SItem> m_NumSetShipsDef;

vector<SItem> m_NumShipsInKBAtt[7], m_NumShipsInKBDef[7];
DWORD m_NumShotsPerRoundAtt[6], m_NumShotsPerRoundDef[6];
double m_ShotStrengthAtt[6], m_ShotStrengthDef[6];
double m_AbsorbedAtt[6], m_AbsorbedDef[6];
DWORD m_NumSimulatedRounds[7];

#ifdef CREATE_ADV_STATS
// number of ships after battle
vector<vector<int> > m_CombatResultsAtt, m_CombatResultsDef;
// losses / win
vector<Res> m_LossAtt, m_LossDef;
vector<Res> m_DebrisFields;
//vector<Res> m_
#endif

int m_CurrentRound;
int m_CurrentSim;

vector<Obj>* m_AttObj, *m_DefObj;
int m_NumPlayersPerTeam[2];

int m_BestCaseAtt[T_END];
int m_BestCaseDef[T_END];
int m_WorstCaseAtt[T_END];
int m_WorstCaseDef[T_END];

BattleResult m_Result;
WaveInfo m_Waves;

bool m_DefInTF;
bool m_CompBestWorstCase;
bool m_RebuildSmallDef;

bool m_DataIsDeleted;
bool m_SimulateFreedItsData;
bool m_InitSim;
bool m_LastScanHadTechs;
bool m_BracketNames;
bool m_UseOldBS;

int m_LossesToDF;
float m_DefRebuildFac;
bool m_ShipDataFromFile;

// translations strings
genstring m_FleetNames[T_END + 2];
genstring m_altFleetNames[T_END + 1];
genstring m_KBNames[T_END + 1];
// techs: 1=weap, 2=shield, 3=hull, 4=combustion, 5=Impuls, 6=hyperspace
genstring m_TechNames[6];
// stop string for esp. reports
genstring m_EspLimiter;
// espionage report titles 1=Fleets, 2=Defence, 3=Reseach
genstring m_EspTitles[3];
// resources: 1= Metal, 2= Crystal, 3 = Deuterium
genstring m_Ress[3];
// mines: 1=metal, 2=crystal, 3=deuterium
genstring m_wrongRess[3];
// storages: 1=metal 2=crystal, 3=deuerium
genstring m_wrongRess2[3];

// 1="auf ", 2="] am " 3 = Research 4 = Lunar Base
genstring m_Spiostrings[4];
// 1= "dem Planeten ", 2 = "] Die Flotte"
genstring m_Ovr[2];
// text before number of defense units
genstring m_Defense;
// text before the technology level
genstring m_TechPreString;
genstring m_Attacker, m_Defender;

// CR-table down from top
genstring m_KBTable[5];
// 1-weapons, 2-shields, 3-hull
genstring m_KBTechs[3];
// texts between rounds
genstring m_KBRoundStr[4];
genstring m_KBResult[9];
genstring m_KBLoss[3];
genstring m_KBMoon;
genstring m_KBTitle;
// Charset for correct output
genstring m_HTML_Charset;

genstring m_ThousandSep;

genstring m_BWC_CSS, m_CR_CSS;

genstring m_Bilanzstrings[10];

// 0-min, 1-Max, 2-average, 3-Sum, 4 -debris 5- ship, 6- after rebuild 7-best 8-worst
genstring m_BWTable[9];
// 1-title 2-Best/Worst Case for attacker, 3- ..for defender
genstring m_BWTitle[3];
// losses: 1:title, 2:attacker, 3:defender 4:units
genstring m_BWLoss[4];

PlaniPos m_OwnPos[MAX_PLAYERS_PER_TEAM];

TargetInfo m_DefenderInfos[MAX_PLAYERS_PER_TEAM];
// needed for waves
TargetInfo m_LastReadTarget;

// parsing order of normal and alternative ship names
vector<ITEM_TYPE> m_ParseOrder[2];

ShipTechs m_TechsAtt[MAX_PLAYERS_PER_TEAM];
ShipTechs m_TechsDef[MAX_PLAYERS_PER_TEAM];
int m_TechsTW[MAX_PLAYERS_PER_TEAM][3];

// fleet speed (100% = 10)
int m_Speed[MAX_PLAYERS_PER_TEAM];

// RF-values read from file
unsigned long m_RF[T_END][T_END];

// names of ships in ini-files
genstring m_IniFleetNames[T_END];
// unit costs
Res Kosten[T_END];
// max. shields
float MaxShields[MAX_PLAYERS_PER_TEAM + 1][2][T_END];
// max. life
float MaxLifes[MAX_PLAYERS_PER_TEAM + 1][2][T_END];
// damage
float Dams[MAX_PLAYERS_PER_TEAM + 1][2][T_END];
// capacities
long LadeKaps[T_END];
// fuel usage
WORD Verbrauch[T_END];
// ship engines
TRIEBWERK Triebwerke[T_END];
int BaseSpeed[T_END];

#ifdef ASMRAND
DWORD randbuf[34];
DWORD p1, p2;
#else
DWORD randbuf[17][2];
int p1, p2;
#endif

void main(int argc, char* argv[]) 
{
	string line;
	ifstream data_file;
	data_file.exceptions(ifstream::failbit);
	bool isDataAccessed = false;
	while (!isDataAccessed) {
		cout << "\nTrying to open data file...";

		try {
			data_file.open("data.txt");

			if (data_file.is_open())
			{
				isDataAccessed = true;
				FillRFTable(RF_075);
				int i, num_ships = 0, num_defs = 0;
				m_NumShipsAtt.resize(T_END);
				m_NumShipsDef.resize(T_END);

				for (i = 0; i < 7; i++)
				{
					m_NumShipsInKBAtt[i].resize(T_END);
					m_NumShipsInKBDef[i].resize(T_END);
				}
				SItem item;
				cout << "\nFleet numbers: ";
				getline(data_file, line);
				cout << line;
				//put the fleet into an int vector: lf, hf, c, bs, b, d, bc
				vector<int> fleet_vect;
				stringstream fleet_ss(line);
				while (fleet_ss >> i)
				{
					fleet_vect.push_back(i);

					if (fleet_ss.peek() == ',')
						fleet_ss.ignore();
				}
				//put the fleet int vector into a vector of ship items
				item.OwnerID = 0;
				vector<SItem> fleet;
				for (i = 0; i < T_SHIPEND; i++)
				{
					item.Type = (ITEM_TYPE)i;
					item.Num = fleet_vect[i];
					fleet.push_back(item);
				}
				m_AttObj = new vector<Obj>;
				cout << "\nDefense numbers: ";
				getline(data_file, line);
				cout << line;
				//put the defense into an int vector: rl, ll, hl, gc, ic, pt
				vector<int> defense_vect;
				stringstream defense_ss(line);
				while (defense_ss >> i)
				{
					defense_vect.push_back(i);

					if (defense_ss.peek() == ',')
						defense_ss.ignore();
				}
				//put the defense int vector into a vector of ship items
				item.OwnerID = 6;
				vector<SItem> defense;
				for (i = 0; i < T_END; i++)
				{
					item.Type = (ITEM_TYPE)i;
					item.Num = defense_vect[i];
					defense.push_back(item);
				}
				//add domes to defense
				m_DefObj = new vector<Obj>;
				SetFleet(&fleet, &defense);
				getline(data_file, line);
				int num_sims = stoi(line);
				Simulate(num_sims);

				ofstream result_File;
				result_File.exceptions(ifstream::failbit);
				bool isResultWritten = false;
				while (!isResultWritten) {
					cout << "\nTrying to open result file...";
					try {
						result_File.open("result.txt", std::ofstream::trunc);
						if (result_File.is_open())
						{
							result_File << m_Result.VerlusteAngreifer.met;
							result_File << "\n";
							result_File << m_Result.VerlusteAngreifer.kris;
							result_File << "\n";
							result_File << m_Result.VerlusteAngreifer.deut;
							result_File << "\n";
							result_File << (m_Result.AttWon * 100);
							result_File << "\n";
							result_File.close();
							isResultWritten = true;

							cout << "\nSimulation complete.\nAttacker win chance: ";
							cout << m_Result.AttWon;
							cout << "\nDefender win chance: ";
							cout << m_Result.DefWon;
							cout << "\nDraw chance: ";
							cout << m_Result.Draw;
							cout << "\nAttacker losses: ";
							cout << m_Result.VerlusteAngreifer.met;
							cout << " Met, ";
							cout << m_Result.VerlusteAngreifer.kris;
							cout << " Crys, ";
							cout << m_Result.VerlusteAngreifer.deut;
							cout << " Deut";
						}
					}
					catch (const ifstream::failure& e) {
						cout << "Exception opening/writing result file";
						ofstream error_File;
						error_File.open("error.txt");
						error_File << "Unable to open result file, exiting...";
					}
				}
			}
		}	
		catch (const ifstream::failure& fail) {
			cout << "Exception opening/reading data file";
			ofstream error_File;
			error_File.open("error.txt");
			error_File << "Unable to open data file, exiting...";
		}
	}
}

// creates an object with the correct values
Obj FillObj(ITEM_TYPE Type, int Team, DWORD PlayerID)
{
	Obj o;
	o.Type = Type;

	o.Life = MaxLifes[PlayerID][Team][Type];
	o.Shield = MaxShields[PlayerID][Team][Type];
	o.Explodes = false;
	o.PlayerID = PlayerID;
	return o;
}


/// simulate the battle
bool Simulate(int count)
{
	bool m_InitSim = false;

	bool m_DataIsDeleted = false;
	bool m_SimulateFreedItsData = false;

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


bool InitSim()
{
	PR_PROF_FUNC(F_INITSIM);

	// more randomness
	InitRand();
	srand(time(NULL));

	// calculate cost, hull, shield etc. of ships
	ComputeShipData();

	Obj obj;
	DWORD CurrPl = 999;
	unsigned int item = 0;
	int i;
	// reset fleet
	m_AttObj->clear();
	m_DefObj->clear();

	m_NumShipsAtt.resize(T_RAK*MAX_PLAYERS_PER_TEAM);
	m_NumShipsDef.resize((T_END)*MAX_PLAYERS_PER_TEAM);
	for (i = 0; i < MAX_PLAYERS_PER_TEAM; i++)
	{
		for (int j = 0; j < T_SHIPEND; j++)
		{
			m_NumShipsAtt[i*T_SHIPEND + j].OwnerID = i;
			m_NumShipsAtt[i*T_SHIPEND + j].Type = (ITEM_TYPE)j;
			m_NumShipsAtt[i*T_SHIPEND + j].Num = 0;
		}
	}
	for (i = 0; i < MAX_PLAYERS_PER_TEAM; i++)
	{
		for (int j = 0; j < T_END; j++)
		{
			m_NumShipsDef[i*(T_END)+j].OwnerID = i;
			m_NumShipsDef[i*(T_END)+j].Type = (ITEM_TYPE)j;
			m_NumShipsDef[i*(T_END)+j].Num = 0;
		}
	}
	for (item = 0; item < m_NumSetShipsAtt.size(); item++)
	{
		// create attacking objects; for every ship 1 object
		obj = FillObj(m_NumSetShipsAtt[item].Type, ATTER, m_NumSetShipsAtt[item].OwnerID);
		if (m_NumSetShipsAtt[item].OwnerID != CurrPl)
			CurrPl = m_NumSetShipsAtt[item].OwnerID;
		for (size_t o = 0; o < m_NumSetShipsAtt[item].Num; o++)
			m_AttObj->push_back(obj);
	}
	if (CurrPl < MAX_PLAYERS_PER_TEAM)
		m_NumPlayersPerTeam[ATTER] = CurrPl + 1;
	else
		m_NumPlayersPerTeam[ATTER] = 0;

	CurrPl = 999;
	for (item = 0; item < m_NumSetShipsDef.size(); item++)
	{
		// defending units
		obj = FillObj(m_NumSetShipsDef[item].Type, DEFFER, m_NumSetShipsDef[item].OwnerID);
		if (m_NumSetShipsDef[item].OwnerID != CurrPl)
			CurrPl = m_NumSetShipsDef[item].OwnerID;
		for (size_t o = 0; o < m_NumSetShipsDef[item].Num; o++)
		{
			m_DefObj->push_back(obj);
		}
	}
	if (CurrPl < MAX_PLAYERS_PER_TEAM)
		m_NumPlayersPerTeam[DEFFER] = CurrPl + 1;
	else
		m_NumPlayersPerTeam[DEFFER] = 0;
	m_Result.NumRounds = 0;
	m_Result.TF = Res();
	m_Result.MaxTF = Res();
	m_Result.VerlusteAngreifer = Res();
	m_Result.VerlusteVerteidiger = Res();
	m_Result.VerlVertmitAufbau = Res();
	m_Result.MaxVerlAtt = Res();
	m_Result.MaxVerlDef = Res();
	m_Result.MinTF = Res();
	m_Result.MinVerlAtt = Res();
	m_Result.MinVerlDef = Res();
	m_Result.Beute = Res();
	m_Result.AttWon = 0;
	m_Result.DefWon = 0;
	m_Result.Draw = 0;
	m_Result.SpritVerbrauch = 0;
	m_Result.Ausbeute = 0;
	m_Result.WertAtt = Res();
	m_Result.WertDef = Res();
	// reset best/worst case
	for (int u = 0; u < T_END; u++)
	{
		m_WorstCaseAtt[u] = 999999999;
		m_BestCaseAtt[u] = 0;
		m_WorstCaseDef[u] = 999999999;
		m_BestCaseDef[u] = 0;
	}
	// reset combat report data
	for (int r = 0; r < 7; r++)
	{
		for (int i = 0; i < T_END; i++)
		{
			m_NumShipsInKBAtt[r][i].Num = 0;
			m_NumShipsInKBAtt[r][i].Type = (ITEM_TYPE)i;
			m_NumShipsInKBDef[r][i].Num = 0;
			m_NumShipsInKBDef[r][i].Type = (ITEM_TYPE)i;
		}

		m_NumSimulatedRounds[r] = 0;

		if (r < 6)
		{
			m_NumShotsPerRoundAtt[r] = 0;
			m_NumShotsPerRoundDef[r] = 0;
			m_ShotStrengthAtt[r] = 0;
			m_ShotStrengthDef[r] = 0;
			m_AbsorbedAtt[r] = 0;
			m_AbsorbedDef[r] = 0;
		}

	}
	return true;
}


void MaxAllShields()
{
	PR_PROF_FUNC(F_MASHIELDS);
	// this function sets all shields to its maximum
	size_t i;
	DWORD id;
	for (i = 0; i < m_AttObj->size(); i++) {
		id = (*m_AttObj)[i].PlayerID;
		(*m_AttObj)[i].Shield = MaxShields[id][ATTER][(*m_AttObj)[i].Type];
	}

	for (i = 0; i < m_DefObj->size(); i++) {
		id = (*m_DefObj)[i].PlayerID;
		(*m_DefObj)[i].Shield = MaxShields[id][DEFFER][(*m_DefObj)[i].Type];
	}
}

bool CanShootAgain_FromTable(ITEM_TYPE AttType, ITEM_TYPE TargetType)
{
    return RandomNumber(10000) >= m_RF[AttType][TargetType];
}


// computes new best/worst case
void UpdateBestWorstCase(int CurSim)
{
	PR_PROF_FUNC(F_ADDPTONUMBER);

	if (!m_CompBestWorstCase)
		return;

	int a[T_END], d[T_END];
	size_t i;
	for (i = 0; i < T_END; i++)
	{
		a[i] = 0;
		d[i] = 0;
	}

	// count ships
	for (i = 0; i < m_AttObj->size(); i++)
		a[(*m_AttObj)[i].Type]++;

	for (i = 0; i < m_DefObj->size(); i++)
		d[(*m_DefObj)[i].Type]++;

	// check if better / worse
	for (i = 0; i < T_END; i++)
	{
		if (m_BestCaseAtt[i] < a[i])
			m_BestCaseAtt[i] = a[i];
		if (m_WorstCaseAtt[i] > a[i])
			m_WorstCaseAtt[i] = a[i];

		if (m_BestCaseDef[i] < d[i])
			m_BestCaseDef[i] = d[i];
		if (m_WorstCaseDef[i] > d[i])
			m_WorstCaseDef[i] = d[i];

#ifdef CREATE_ADV_STATS
		// save result
		m_CombatResultsAtt[CurSim][i] = a[i];
		m_CombatResultsDef[CurSim][i] = d[i];
#endif
	}
}


void ComputeCRArrays()
{
	PR_PROF_FUNC(F_CKBARRAYS);

	for (int r = 0; r < 7; r++)
	{
		for (int t = 0; t < T_END; t++)
		{
			if (m_NumSimulatedRounds[r] > 0)
			{
				m_NumShipsInKBAtt[r][t].Num /= m_NumSimulatedRounds[r];
				if (m_NumShipsInKBAtt[r][t].Num > 0)
					m_NumShipsInKBAtt[r][t].Num += 0.5f;
				m_NumShipsInKBDef[r][t].Num /= m_NumSimulatedRounds[r];
				if (m_NumShipsInKBDef[r][t].Num > 0)
					m_NumShipsInKBDef[r][t].Num += 0.5f;
			}
			else
			{
				m_NumShipsInKBAtt[r][t].Num = 0;
				m_NumShipsInKBDef[r][t].Num = 0;
			}
		}
		if (r < 6)
		{
			if (m_NumSimulatedRounds[r] > 0)
			{
				m_NumShotsPerRoundAtt[r] /= m_NumSimulatedRounds[r];
				m_NumShotsPerRoundDef[r] /= m_NumSimulatedRounds[r];
				m_ShotStrengthAtt[r] /= m_NumSimulatedRounds[r];
				m_ShotStrengthDef[r] /= m_NumSimulatedRounds[r];
				m_AbsorbedAtt[r] /= m_NumSimulatedRounds[r];
				m_AbsorbedDef[r] /= m_NumSimulatedRounds[r];
			}
		}
	}
}


// removes destroyed ships from the arrays
void DestroyExplodedShips()
{
	PR_PROF_FUNC(F_DESTEXPLSHIPS);

	vector<Obj>* tmpAtt = new vector<Obj>;
	vector<Obj>* tmpDef = new vector<Obj>;

	DWORD Num = 0;
	size_t i;
	for (i = 0; i < m_AttObj->size(); i++)
	{
		if ((*m_AttObj)[i].Explodes == false)
			Num++;
	}

	DWORD count = 0;

	tmpAtt->resize(Num);
	for (i = 0; i < m_AttObj->size(); i++)
	{
		if (!(*m_AttObj)[i].Explodes)
			(*tmpAtt)[count++] = (*m_AttObj)[i];
	}

	Num = 0;
	count = 0;
	for (i = 0; i < m_DefObj->size(); i++)
	{
		if ((*m_DefObj)[i].Explodes == false)
			Num++;
	}

	tmpDef->resize(Num);
	for (i = 0; i < m_DefObj->size(); i++)
	{
		if (!(*m_DefObj)[i].Explodes)
			(*tmpDef)[count++] = (*m_DefObj)[i];
	}

	delete m_AttObj;
	delete m_DefObj;
	m_AttObj = tmpAtt;
	m_DefObj = tmpDef;
}


// sets values for every unit (shield, life, hull etc.)
void ComputeShipData()
{
	if (!m_ShipDataFromFile) {
		Kosten[T_KT] = Res(2000, 2000);
		Kosten[T_GT] = Res(6000, 6000);
		Kosten[T_LJ] = Res(3000, 1000);
		Kosten[T_SJ] = Res(6000, 4000);
		Kosten[T_KREUZER] = Res(20000, 7000, 2000);
		if (!m_UseOldBS)
			Kosten[T_SS] = Res(45000, 15000);
		else
			Kosten[T_SS] = Res(40000, 20000);
		Kosten[T_KOLO] = Res(10000, 20000, 10000);
		Kosten[T_REC] = Res(10000, 6000, 2000);
		Kosten[T_SPIO] = Res(0, 1000);
		Kosten[T_BOMBER] = Res(50000, 25000, 15000);
		Kosten[T_SAT] = Res(0, 2000, 500);
		Kosten[T_ZER] = Res(60000, 50000, 15000);
		Kosten[T_TS] = Res(5000000, 4000000, 1000000);
		Kosten[T_IC] = Res(30000, 40000, 15000);
		Kosten[T_RAK] = Res(2000, 0);
		Kosten[T_LL] = Res(1500, 500);
		Kosten[T_SL] = Res(6000, 2000);
		Kosten[T_GAUSS] = Res(20000, 15000, 2000);
		Kosten[T_IONEN] = Res(2000, 6000);
		Kosten[T_PLASMA] = Res(50000, 50000, 30000);
		Kosten[T_KS] = Res(10000, 10000);
		Kosten[T_GS] = Res(50000, 50000);

		MaxShields[MAX_PLAYERS_PER_TEAM][ATTER][T_KT] = 10;
		MaxShields[MAX_PLAYERS_PER_TEAM][ATTER][T_GT] = 25;
		MaxShields[MAX_PLAYERS_PER_TEAM][ATTER][T_LJ] = 10;
		MaxShields[MAX_PLAYERS_PER_TEAM][ATTER][T_SJ] = 25;
		MaxShields[MAX_PLAYERS_PER_TEAM][ATTER][T_KREUZER] = 50;
		MaxShields[MAX_PLAYERS_PER_TEAM][ATTER][T_SS] = 200;
		MaxShields[MAX_PLAYERS_PER_TEAM][ATTER][T_KOLO] = 100;
		MaxShields[MAX_PLAYERS_PER_TEAM][ATTER][T_REC] = 10;
		MaxShields[MAX_PLAYERS_PER_TEAM][ATTER][T_SPIO] = 0.01f;
		MaxShields[MAX_PLAYERS_PER_TEAM][ATTER][T_BOMBER] = 500;
		MaxShields[MAX_PLAYERS_PER_TEAM][ATTER][T_SAT] = 1;
		MaxShields[MAX_PLAYERS_PER_TEAM][ATTER][T_ZER] = 500;
		MaxShields[MAX_PLAYERS_PER_TEAM][ATTER][T_TS] = 50000;
		MaxShields[MAX_PLAYERS_PER_TEAM][ATTER][T_IC] = 400;
		MaxShields[MAX_PLAYERS_PER_TEAM][ATTER][T_RAK] = 20;
		MaxShields[MAX_PLAYERS_PER_TEAM][ATTER][T_LL] = 25;
		MaxShields[MAX_PLAYERS_PER_TEAM][ATTER][T_SL] = 100;
		MaxShields[MAX_PLAYERS_PER_TEAM][ATTER][T_GAUSS] = 200;
		MaxShields[MAX_PLAYERS_PER_TEAM][ATTER][T_IONEN] = 500;
		MaxShields[MAX_PLAYERS_PER_TEAM][ATTER][T_PLASMA] = 300;
		MaxShields[MAX_PLAYERS_PER_TEAM][ATTER][T_KS] = 2000;
		MaxShields[MAX_PLAYERS_PER_TEAM][ATTER][T_GS] = 10000;

		Dams[MAX_PLAYERS_PER_TEAM][ATTER][T_KT] = 5;
		Dams[MAX_PLAYERS_PER_TEAM][ATTER][T_GT] = 5;
		Dams[MAX_PLAYERS_PER_TEAM][ATTER][T_LJ] = 50;
		Dams[MAX_PLAYERS_PER_TEAM][ATTER][T_SJ] = 150;
		Dams[MAX_PLAYERS_PER_TEAM][ATTER][T_KREUZER] = 400;
		Dams[MAX_PLAYERS_PER_TEAM][ATTER][T_SS] = 1000;
		Dams[MAX_PLAYERS_PER_TEAM][ATTER][T_KOLO] = 50;
		Dams[MAX_PLAYERS_PER_TEAM][ATTER][T_REC] = 1;
		Dams[MAX_PLAYERS_PER_TEAM][ATTER][T_SPIO] = 0.01f;
		Dams[MAX_PLAYERS_PER_TEAM][ATTER][T_BOMBER] = 1000;
		Dams[MAX_PLAYERS_PER_TEAM][ATTER][T_SAT] = 1;
		Dams[MAX_PLAYERS_PER_TEAM][ATTER][T_ZER] = 2000;
		Dams[MAX_PLAYERS_PER_TEAM][ATTER][T_TS] = 200000;
		Dams[MAX_PLAYERS_PER_TEAM][ATTER][T_IC] = 700;
		Dams[MAX_PLAYERS_PER_TEAM][ATTER][T_RAK] = 80;
		Dams[MAX_PLAYERS_PER_TEAM][ATTER][T_LL] = 100;
		Dams[MAX_PLAYERS_PER_TEAM][ATTER][T_SL] = 250;
		Dams[MAX_PLAYERS_PER_TEAM][ATTER][T_GAUSS] = 1100;
		Dams[MAX_PLAYERS_PER_TEAM][ATTER][T_IONEN] = 150;
		Dams[MAX_PLAYERS_PER_TEAM][ATTER][T_PLASMA] = 3000;
		Dams[MAX_PLAYERS_PER_TEAM][ATTER][T_KS] = 1;
		Dams[MAX_PLAYERS_PER_TEAM][ATTER][T_GS] = 1;
	}
	int i;
	for (i = 0; i < T_END; i++)
	{
		MaxLifes[MAX_PLAYERS_PER_TEAM][ATTER][i] = (Kosten[i].kris + Kosten[i].met) / 10.0f;
	}

	for (DWORD ID = 0; ID < MAX_PLAYERS_PER_TEAM; ID++)
	{
		double DamFak_a = (10 + m_TechsAtt[ID].Weapon) / 10.0f;
		double ShFak_a = (10 + m_TechsAtt[ID].Shield) / 10.0f;
		double LifeFak_a = (10 + m_TechsAtt[ID].Armour) / 10.0f;

		double DamFak_v = (10 + m_TechsDef[ID].Weapon) / 10.0f;
		double ShFak_v = (10 + m_TechsDef[ID].Shield) / 10.0f;
		double LifeFak_v = (10 + m_TechsDef[ID].Armour) / 10.0f;

		for (i = 0; i < T_END; i++)
		{
			MaxLifes[ID][DEFFER][i] = MaxLifes[MAX_PLAYERS_PER_TEAM][ATTER][i];
			MaxShields[ID][DEFFER][i] = MaxShields[MAX_PLAYERS_PER_TEAM][ATTER][i];
			Dams[ID][DEFFER][i] = Dams[MAX_PLAYERS_PER_TEAM][ATTER][i];

			MaxLifes[ID][ATTER][i] = MaxLifes[MAX_PLAYERS_PER_TEAM][ATTER][i];
			MaxShields[ID][ATTER][i] = MaxShields[MAX_PLAYERS_PER_TEAM][ATTER][i];
			Dams[ID][ATTER][i] = Dams[MAX_PLAYERS_PER_TEAM][ATTER][i];


			MaxLifes[ID][DEFFER][i] = floor(MaxLifes[ID][DEFFER][i] * LifeFak_v);
			MaxShields[ID][DEFFER][i] = floor(MaxShields[ID][DEFFER][i] * ShFak_v);
			Dams[ID][DEFFER][i] = floor(Dams[ID][DEFFER][i] * DamFak_v);

			MaxLifes[ID][ATTER][i] = floor(MaxLifes[ID][ATTER][i] * LifeFak_a);
			MaxShields[ID][ATTER][i] = floor(MaxShields[ID][ATTER][i] * ShFak_a);
			Dams[ID][ATTER][i] = floor(Dams[ID][ATTER][i] * DamFak_a);
		}
	}
	if (!m_ShipDataFromFile)
	{
		for (i = 0; i < T_END; i++)
		{
			LadeKaps[i] = 0;
			Verbrauch[i] = 0;
			BaseSpeed[i] = 0;
		}
		// ship capacity
		LadeKaps[T_KT] = 5000;
		LadeKaps[T_GT] = 25000;
		LadeKaps[T_LJ] = 50;
		LadeKaps[T_SJ] = 100;
		LadeKaps[T_KREUZER] = 800;
		LadeKaps[T_SS] = 1500;
		LadeKaps[T_KOLO] = 7500;
		LadeKaps[T_REC] = 20000;
		 //changed since OGame 0.74a
		LadeKaps[T_SPIO] = 0;
		LadeKaps[T_BOMBER] = 500;
		LadeKaps[T_ZER] = 2000;
		LadeKaps[T_TS] = 1000000;
		LadeKaps[T_IC] = 750;

		// consumption
		Verbrauch[T_KT] = 10;
		Verbrauch[T_GT] = 50;
		Verbrauch[T_LJ] = 20;
		Verbrauch[T_SJ] = 75;
		Verbrauch[T_KREUZER] = 300;
		Verbrauch[T_SS] = 500;
		Verbrauch[T_KOLO] = 1000;
		Verbrauch[T_REC] = 300;
		Verbrauch[T_SPIO] = 1;
		Verbrauch[T_BOMBER] = 1000;
		Verbrauch[T_ZER] = 1000;
		Verbrauch[T_TS] = 1;
		Verbrauch[T_IC] = 250;

		// speed
		BaseSpeed[T_KT] = 5000;
		BaseSpeed[T_GT] = 7500;
		BaseSpeed[T_LJ] = 12500;
		BaseSpeed[T_SJ] = 10000;
		BaseSpeed[T_KREUZER] = 15000;
		BaseSpeed[T_SS] = 10000;
		BaseSpeed[T_KOLO] = 2500;
		BaseSpeed[T_REC] = 2000;
		BaseSpeed[T_SPIO] = 100000000;
		BaseSpeed[T_BOMBER] = 4000;
		BaseSpeed[T_ZER] = 5000;
		BaseSpeed[T_TS] = 100;
		BaseSpeed[T_IC] = 10000;
	}

	Triebwerke[T_KT] = TW_VERBRENNUNG;
	Triebwerke[T_GT] = TW_VERBRENNUNG;
	Triebwerke[T_LJ] = TW_VERBRENNUNG;
	Triebwerke[T_SJ] = TW_IMPULS;
	Triebwerke[T_KREUZER] = TW_IMPULS;
	Triebwerke[T_SS] = TW_HYPERRAUM;
	Triebwerke[T_KOLO] = TW_IMPULS;
	Triebwerke[T_REC] = TW_VERBRENNUNG;
	Triebwerke[T_SPIO] = TW_VERBRENNUNG;
	Triebwerke[T_BOMBER] = TW_IMPULS;
	Triebwerke[T_ZER] = TW_HYPERRAUM;
	Triebwerke[T_TS] = TW_HYPERRAUM;
	Triebwerke[T_IC] = TW_HYPERRAUM;
}


// lets shoot a ship
void ShipShoots(Obj& o, int Team, DWORD AtterID)
{
	PR_BEG_FUNC(F_SSHOOTS);
	bool ShootsAgain = true;
	ULONG Ziel = 0;
	int ZielTeam = Team == ATTER ? DEFFER : ATTER;
	DWORD DefferID = 0;

	double Dam = Dams[AtterID][Team][o.Type];
	double Dam2 = Dam;

	Obj* obj;

	vector<Obj>* treffer = (Team == ATTER ? m_DefObj : m_AttObj);

	unsigned int ListSize = treffer->size();
	if (ListSize == 0)
		return;

	// shoot until RF stops 
	while (ShootsAgain)
	{
		Dam = Dams[AtterID][Team][o.Type];
		Dam2 = Dam;
		DefferID = 0;
		{
			//PR_PROF_FUNC(F_RANDNR);
			// get random target from enemy
			Ziel = RandomNumber(ListSize);
			DefferID = (*treffer)[Ziel].PlayerID;
		}

		if (Team == ATTER)
		{
			m_NumShotsPerRoundAtt[m_CurrentRound]++;
			m_ShotStrengthAtt[m_CurrentRound] += Dam;
		}
		else
		{
			m_NumShotsPerRoundDef[m_CurrentRound]++;
			m_ShotStrengthDef[m_CurrentRound] += Dam;
		}

		obj = &(*treffer)[Ziel];
		double max_shield = MaxShields[DefferID][ZielTeam][obj->Type];
		if (Dam < obj->Shield)
		{
			// round damage down to full percents
			double perc = floor(100.0f * Dam / max_shield);
			Dam = max_shield * perc;
			Dam /= 100.0f;

			Dam2 = Dam;
		}
		if (obj->Shield <= 0 || Dam > 0)
		{
			// reduce shield by damage
			Dam -= obj->Shield;
			obj->Shield -= Dam2;
			if (Dam < 0)
				Dam = 0;
		}
		else
		{
			Dam = 0;
		}
		double absorbed = Dams[AtterID][Team][o.Type] - Dam;
		// absorbed damage (for combat reports)
		if (Team == ATTER)
			m_AbsorbedDef[m_CurrentRound] += absorbed;
		else
			m_AbsorbedAtt[m_CurrentRound] += absorbed;

		if (obj->Shield < 0)
			obj->Shield = 0;
		if (Dam > 0)
		{
			// if damage left, destroy hull
			obj->Life -= Dam;
			if (obj->Life < 0)
				obj->Life = 0;

		}
		if (obj->Life <= 0.7f * MaxLifes[DefferID][ZielTeam][obj->Type])
		{
			// ship probably explodes, when hull damage >= 30 %
			if (rand() % 100 >= 100.f * obj->Life / MaxLifes[DefferID][ZielTeam][obj->Type])
				obj->Explodes = true;
		}
		// can shoot this at ship again?
		ShootsAgain = CanShootAgain_FromTable(o.Type, obj->Type);
	}
	return;
}


// computes losses/debris and battle result
void ComputeBattleResult()
{
	ComputeLosses();
	//ComputeLoot(); //we don't care about loot
	size_t i;
	// remove empty ships
	for (i = 0; i < m_NumShipsAtt.size(); i++) {
		vector<SItem>::iterator it = m_NumShipsAtt.begin() + i;
		if (!it->Num || it->Type == T_NONE) {
			m_NumShipsAtt.erase(it);
			i--;
		}
	}
	for (i = 0; i < m_NumShipsDef.size(); i++) {
		vector<SItem>::iterator it = m_NumShipsDef.begin() + i;
		if (!it->Num || it->Type == T_NONE) {
			m_NumShipsDef.erase(it);
			i--;
		}
	}

	if (m_Result.VerlusteAngreifer.met != 0)
		m_Result.PercentInTFMet = 100 * ((float)m_Result.TF.met / m_Result.VerlusteAngreifer.met);
	else
		m_Result.PercentInTFMet = 0;

	if (m_Result.VerlusteAngreifer.kris != 0)
		m_Result.PercentInTFKris = 100 * ((float)m_Result.TF.kris / m_Result.VerlusteAngreifer.kris);
	else
		m_Result.PercentInTFKris = 0;

	if (m_Result.Ausbeute > 100)
		m_Result.Ausbeute = 100;

	// fuel calculation
	m_Result.FlyTime = 0;
	if (m_Result.Position.Gala)
	{
		m_Result.SpritVerbrauch = 0;
		m_Result.FlyTime = 0;
		float minSpeed = 9999999.0f;
		unsigned int uiTime = 0;
		// get minimum speed of all fleets
		//we don't care about speed
		//for (int i = 0; i < MAX_PLAYERS_PER_TEAM; i++) {
		//	float sp = GetFleetSpeed(i, m_NumSetShipsAtt);
		//	if (sp < minSpeed)
		//		minSpeed = sp;
		//}
		for (int pl = 0; pl < MAX_PLAYERS_PER_TEAM; pl++)
		{
			/*
			//we don't care about fuel
			int dist = GetDistance(m_OwnPos[pl], m_Result.Position);

			// fly time for recyclers
			vector<SItem> vRec;
			vRec.push_back(SItem(T_REC, 1, pl));
			m_Result.RecFlyTime[pl] = ComputeFlyTime(m_OwnPos[pl], m_Result.Position, pl, vRec);
			// fuel for recyclers
			float rec_bsp = BaseSpeed[T_REC] * (1 + (float)m_TechsTW[pl][Triebwerke[T_REC]] * (Triebwerke[T_REC] + 1) / 10.0f);
			float rec_sp = 35000.f / (m_Result.RecFlyTime[pl] - 10) * sqrt(dist * 10 / rec_bsp);
			m_Result.RecFuel[pl] = m_Result.NumRecs * Verbrauch[T_REC] * (rec_sp / 10.0f + 1) * (rec_sp / 10.0f + 1) * dist / 35000.0f + 1;

			// compute separate fuel need for every fleet
			uiTime = ComputeFlyTime(m_OwnPos[pl], m_Result.Position, pl, m_NumSetShipsAtt);
			if (m_OwnPos[pl].Gala == 0 && m_OwnPos[pl].Sys == 0 && m_OwnPos[pl].Pos == 0)
				continue;
			__int64 gesverb = 0;
			// sum consumptions
			*/
			// OGame >= v0.68a
			//for (size_t i = 0; i < m_NumSetShipsAtt.size(); i++)
			//{
			//	if (m_NumSetShipsAtt[i].OwnerID == pl) {
			//		int type = m_NumSetShipsAtt[i].Type;
			//		float basesp = BaseSpeed[type];
			//		int engine = Triebwerke[type];
			//		int iConsumpt = Verbrauch[type];
			//		if (type == T_KT && m_TechsTW[pl][engine + 1] >= 5)
			//		{
			//			basesp *= 2;
			//			engine += 1;
			//			iConsumpt *= 2;
			//		}
			//		if (type == T_BOMBER && m_TechsTW[pl][engine + 1] >= 8) {
			//			basesp = 5000;
			//			engine += 1;
			//		}
			//		basesp *= (1 + (float)m_TechsTW[pl][engine] * (engine + 1) / 10.0f);
			//		float sp = 35000.f / (uiTime - 10) * sqrt(dist * 10 / basesp);
			//		gesverb += m_NumSetShipsAtt[i].Num * iConsumpt * (sp / 10.0f + 1) * (sp / 10.0f + 1);
			//	}
			//}
			//m_Result.SpritVerbrauch += gesverb * dist / 35000.0f + 1;
			// get slowest fleet
			if (uiTime > m_Result.FlyTime)
				m_Result.FlyTime = uiTime;
		}
	}
	else
		m_Result.SpritVerbrauch = m_Result.FlyTime = 0;

	m_Result.GewinnOhneTF = m_Result.Beute - m_Result.VerlusteAngreifer;
	m_Result.GewinnMitHalfTF = m_Result.Beute + m_Result.TF * 0.5f - m_Result.VerlusteAngreifer;
	m_Result.GewinnMitTF = m_Result.Beute + m_Result.TF - m_Result.VerlusteAngreifer;
	m_Result.GewinnOhneTF.deut -= m_Result.SpritVerbrauch;
	m_Result.GewinnMitHalfTF.deut -= m_Result.SpritVerbrauch;
	m_Result.GewinnMitTF.deut -= m_Result.SpritVerbrauch;

	m_Result.GewinnOhneTF_def = m_Result.Beute * -1 - m_Result.VerlVertmitAufbau;
	m_Result.GewinnMitHalfTF_def = m_Result.TF * 0.5 - m_Result.Beute - m_Result.VerlVertmitAufbau;
	m_Result.GewinnMitHalfTF_def.deut = -m_Result.Beute.deut - m_Result.VerlVertmitAufbau.deut;
	m_Result.GewinnMitTF_def = m_Result.TF - m_Result.Beute - m_Result.VerlVertmitAufbau;
	m_Result.GewinnMitTF_def.deut = -m_Result.Beute.deut - m_Result.VerlVertmitAufbau.deut;
	m_Result.TotalFuel = m_Waves.TotalFuel + m_Result.SpritVerbrauch;
}

void ComputeLosses()
{
	size_t i = 0;
	Res verl;
	vector<SItem> anf, uebrig;

	// count all ships - attacker
	anf.resize(T_END); uebrig.resize(T_END);
	for (i = 0; i < T_END; i++)
	{
		anf[i].Type = (ITEM_TYPE)i;
		uebrig[i].Type = (ITEM_TYPE)i;
		anf[i].Num = 0;
		uebrig[i].Num = 0;
	}

	for (i = 0; i < m_NumSetShipsAtt.size(); i++)
		anf[m_NumSetShipsAtt[i].Type].Num += m_NumSetShipsAtt[i].Num;
	for (i = 0; i < m_NumShipsAtt.size(); i++)
		uebrig[m_NumShipsAtt[i].Type].Num += m_NumShipsAtt[i].Num;

	for (i = 0; i < anf.size(); i++)
	{
		DWORD Type = anf[i].Type;
		DWORD SetShips = anf[i].Num;
		if (SetShips == 0)
			continue;
		verl = Kosten[Type] * (SetShips - ceil(uebrig[i].Num - 0.5));
		m_Result.VerlusteAngreifer += verl;

		if (i < T_SHIPEND)
			m_Result.TF += verl * m_LossesToDF / 100;
		else if (m_DefInTF)
			m_Result.TF += verl * m_LossesToDF / 100;

		if (m_CompBestWorstCase)
		{
			// Worst-Case
			verl = Kosten[Type] * (anf[i].Num - m_WorstCaseAtt[i]);
			m_Result.MaxVerlAtt += verl;
			if (i < T_SHIPEND)
				m_Result.MaxTF += verl * m_LossesToDF / 100;
			else if (m_DefInTF)
				m_Result.MaxTF += verl * m_LossesToDF / 100;

			// Best-Case
			verl = Kosten[Type] * (anf[i].Num - m_BestCaseAtt[i]);
			m_Result.MinVerlAtt += verl;
			if (i < T_SHIPEND)
				m_Result.MinTF += verl * m_LossesToDF / 100;
			else if (m_DefInTF)
				m_Result.MinTF += verl * m_LossesToDF / 100;
		}

		m_Result.WertAtt += Kosten[Type] * anf[i].Num;
	}
#ifdef CREATE_ADV_STATS
	for (i = 0; i < m_CombatResultsAtt.size(); i++)
	{
		// losses information
		Res losses = Res();
		for (j = 0; j < T_END; j++)
		{
			losses += Kosten[j] * (anf[j].Num - m_CombatResultsAtt[i][j]);
		}
		m_LossAtt[i] = losses;
		m_DebrisFields[i] = losses * m_LossesToDF / 100;
	}
#endif

	// count all ships - defender
	anf.resize(T_END); uebrig.resize(T_END);
	for (i = 0; i < T_END; i++)
	{
		anf[i].Type = (ITEM_TYPE)i;
		uebrig[i].Type = (ITEM_TYPE)i;
		anf[i].Num = 0;
		uebrig[i].Num = 0;
	}

	for (i = 0; i < m_NumSetShipsDef.size(); i++)
		anf[m_NumSetShipsDef[i].Type].Num += m_NumSetShipsDef[i].Num;
	for (i = 0; i < m_NumShipsDef.size(); i++)
		uebrig[m_NumShipsDef[i].Type].Num += m_NumShipsDef[i].Num;

	for (i = 0; i < anf.size(); i++)
	{
		verl = Kosten[i] * (anf[i].Num - ceil(uebrig[i].Num - 0.5));
		m_Result.VerlusteVerteidiger += verl;
		if (i < T_SHIPEND)
			m_Result.TF += verl * m_LossesToDF / 100;
		else if (m_DefInTF)
			m_Result.TF += verl * m_LossesToDF / 100;
		if (i < T_SHIPEND)
			m_Result.VerlVertmitAufbau += verl;
		else
			m_Result.VerlVertmitAufbau += verl * (1 - DEF_AUFBAU_FAKTOR);

		if (m_CompBestWorstCase)
		{
			verl = Kosten[i] * (anf[i].Num - m_WorstCaseDef[i]);
			m_Result.MaxVerlDef += verl;
			if (i < T_SHIPEND)
				m_Result.MaxTF += verl * m_LossesToDF / 100;
			else if (m_DefInTF)
				m_Result.MaxTF += verl * m_LossesToDF / 100;


			verl = Kosten[i] * (anf[i].Num - m_BestCaseDef[i]);
			m_Result.MinVerlDef += verl;
			if (i < T_SHIPEND)
				m_Result.MinTF += verl * m_LossesToDF / 100;
			else if (m_DefInTF)
				m_Result.MinTF += verl * m_LossesToDF / 100;
		}

		m_Result.WertDef += Kosten[i] * anf[i].Num;
	}
#ifdef CREATE_ADV_STATS
	for (i = 0; i < m_CombatResultsDef.size(); i++)
	{
		// losses information
		Res losses = Res();
		for (j = 0; j < T_END; j++)
		{
			losses += Kosten[j] * (anf[j].Num - m_CombatResultsDef[i][j]);
		}
		m_LossDef[i] = losses;
		m_DebrisFields[i] += losses * m_LossesToDF / 100;
	}
#endif

	m_Result.NumRecs = ceil((m_Result.TF.met + m_Result.TF.kris) / 20000.0f);
	m_Result.MaxNumRecs = ceil((m_Result.MaxTF.met + m_Result.MaxTF.kris) / 20000.0f);
	m_Result.MinNumRecs = ceil((m_Result.MinTF.met + m_Result.MinTF.kris) / 20000.0f);
	m_Result.TF.deut = 0;
	m_Result.MaxTF.deut = 0;
}


#pragma warning(disable: 4035)
ULONG RandomNumber(ULONG Max)
{
#ifdef ASMRAND
	__asm {
		mov ebx, p1       // ring buffer pointers
		mov ecx, p2       // ring buffer pointer
		mov edx, randbuf[ebx]
		mov eax, randbuf[ebx]
		rol edx, 19       // rotate bits
		rol eax, 27
		add edx, randbuf[ecx]   // add two dwords
		add eax, randbuf[ecx + 4]
		mov randbuf[ebx], eax   // save in swapped order
		mov randbuf[ebx + 4], edx
		sub ebx, 8        // decrement p1
		jnc R30
		mov ebx, 128      // wrap around p1
		R30:
		sub ecx, 8        // decrement p2
			jnc R40
			mov ecx, 128      // wrap around p2
			R40 :
			mov p1, ebx       // save updated pointers
			mov p2, ecx

			mov ebx, Max
			mov ecx, edx      // high bits of random number
			mul ebx           // multiply low 32 bits
			mov eax, ecx
			mov ecx, edx
			mul ebx           // multiply high 32 bits
			mov eax, edx
	}
#else
	DWORD y, z;
	// generate next number
	z = _lrotl(randbuf[p1][0], 19) + randbuf[p2][0];
	y = _lrotl(randbuf[p1][1], 27) + randbuf[p2][1];
	randbuf[p1][0] = y;
	randbuf[p1][1] = z;
	// rotate list pointers
	if (--p1 < 0)
		p1 = 16;
	if (--p2 < 0)
		p2 = 16;
	return y % Max;
#endif
}
#pragma warning(default: 4035)

// inits random number generator
void InitRand() {
	int seed = time(NULL);
#ifdef ASMRAND
	__asm {
		mov eax, seed
		xor ecx, ecx
		R80 :
		imul eax, 2891336453
			inc eax
			mov randbuf[ecx * 4], eax
			inc ecx
			cmp ecx, 34
			jb R80
			mov p1, 0         // initialize buffer pointers
			mov p2, 80
	}
#else
	int i, j;

	for (i = 0; i < 17; i++) {
		for (j = 0; j < 2; j++) {
			seed = seed * 2891336453UL + 1;
			randbuf[i][j] = seed;
		}
	}
	// initialize pointers to circular buffer
	p1 = 0;
	p2 = 10;
#endif
}


// updates number of ships for each round (needed for combat reports)
void SaveShipsToCR(int round)
{
	PR_PROF_FUNC(F_SSTOKB);

	round--;
	m_NumSimulatedRounds[round]++;
	size_t t, i;
	for (i = 0; i < m_AttObj->size(); i++)
	{
		t = (*m_AttObj)[i].Type;
		m_NumShipsInKBAtt[round][t].Num++;
	}

	for (i = 0; i < m_DefObj->size(); i++)
	{
		t = (*m_DefObj)[i].Type;
		m_NumShipsInKBDef[round][t].Num++;
	}
}


// sets a new fleet
bool SetFleet(SItem* Attacker, SItem* Defender, int size_att, int size_def)
{
	vector<SItem> fleet;
	int i;
	if (Attacker && size_att)
	{
		for (i = 0; i < size_att; i++)
			fleet.push_back(Attacker[i]);
		SetFleet(&fleet, NULL);
	}
	if (Defender && size_def)
	{
		for (i = 0; i < size_def; i++)
			fleet.push_back(Defender[i]);
		SetFleet(NULL, &fleet);
	}
	return true;
}

// sets a new fleet
bool SetFleet(vector<SItem>* Attacker, vector<SItem>* Defender)
{
	Obj obj;
	if (Attacker)
		m_AttObj->clear();
	if (Defender)
		m_DefObj->clear();

	srand((unsigned)time(NULL));

	// reduce shield domes to 1
	if (Defender)
	{
		for (size_t i = 0; i < Defender->size(); i++)
		{
			if ((*Defender)[i].Type == T_KS && (*Defender)[i].Num > 1)
				(*Defender)[i].Num = 1;
			if ((*Defender)[i].Type == T_GS && (*Defender)[i].Num > 1)
				(*Defender)[i].Num = 1;
		}
	}

	int StartPos = 0, EndPos = 999;
	int OwnersToSet[MAX_PLAYERS_PER_TEAM], tmp = 0;
	unsigned int NumOwnersToSet = 0;
	size_t i = 0, j = 0;
	if (Attacker) {
		//	    CheckVector(m_NumSetShipsAtt);
		//	    CheckVector(*Attacker);
	}
	if (Attacker && Attacker->size())
	{
		// check, which OwnerIDs should be set new
		for (i = 0; i < MAX_PLAYERS_PER_TEAM; i++)
			OwnersToSet[i] = -1;
		for (i = 0; i < Attacker->size(); i++)
		{
			tmp = (*Attacker)[i].OwnerID;
			for (j = 0; j < NumOwnersToSet; j++)
				if (OwnersToSet[j] == tmp)
					break;
			if (j != NumOwnersToSet && NumOwnersToSet != 0)
				break;

			// owner is not in list of new owners
			OwnersToSet[NumOwnersToSet++] = tmp;
		}

		// Remove all fleets of owners to be reset
		SItem t;
		for (i = 0; i < m_NumSetShipsAtt.size(); i++)
		{
			for (j = 0; j < NumOwnersToSet; j++)
			{
				t = m_NumSetShipsAtt[i];
				if (m_NumSetShipsAtt[i].OwnerID == OwnersToSet[j])
				{
					m_NumSetShipsAtt.erase(m_NumSetShipsAtt.begin() + i);
					i--;
					break;
				}
			}
		}
		// delete 'non-ships'
		for (i = 0; i < Attacker->size(); i++) {
			vector<SItem>::iterator it = Attacker->begin() + i;
			if (!it->Num || it->Type == T_NONE) {
				Attacker->erase(it);
				i--;
			}
		}
		//		CheckVector(m_NumSetShipsAtt);
		// Insert new fleets into fleet vector
		m_NumSetShipsAtt.insert(m_NumSetShipsAtt.end(), Attacker->begin(), Attacker->end());
		sort(m_NumSetShipsAtt.begin(), m_NumSetShipsAtt.end(), ItemCompare);
		//      CheckVector(m_NumSetShipsAtt);
	}

	// repeat algorithm for defender
	NumOwnersToSet = 0, tmp = 0;
	i = 0; j = 0;
	if (Defender && Defender->size())
	{
		for (i = 0; i < MAX_PLAYERS_PER_TEAM; i++)
			OwnersToSet[i] = -1;
		for (i = 0; i < Defender->size(); i++)
		{
			tmp = (*Defender)[i].OwnerID;
			for (j = 0; j < NumOwnersToSet; j++)
				if (OwnersToSet[j] == tmp)
					break;
			if (j != NumOwnersToSet && NumOwnersToSet != 0)
				break;

			OwnersToSet[NumOwnersToSet++] = tmp;
		}
		for (i = 0; i < m_NumSetShipsDef.size(); i++)
		{
			for (j = 0; j < NumOwnersToSet; j++)
			{
				if (m_NumSetShipsDef[i].OwnerID == OwnersToSet[j])
				{
					m_NumSetShipsDef.erase(m_NumSetShipsDef.begin() + i);
					i--;
					break;
				}
			}
		}
		for (i = 0; i < Defender->size(); i++) {
			vector<SItem>::iterator it = Defender->begin() + i;
			if (!it->Num || it->Type == T_NONE) {
				Defender->erase(it);
				i--;
			}
		}
		m_NumSetShipsDef.insert(m_NumSetShipsDef.end(), Defender->begin(), Defender->end());
		sort(m_NumSetShipsDef.begin(), m_NumSetShipsDef.end(), ItemCompare);
		// Update TargetInfo Fleet
		for (j = 0; j < NumOwnersToSet; j++)
		{
			// remove fleet & defence from TargetInfo
			m_DefenderInfos[OwnersToSet[j]].Fleet.clear();
			m_DefenderInfos[OwnersToSet[j]].Defence.clear();
		}
		for (i = 0; i < Defender->size(); i++)
		{
			vector<SItem>::iterator it = Defender->begin() + i;
			if (it->Type < T_SHIPEND)
				m_DefenderInfos[it->OwnerID].Fleet.push_back(*it);
			else
				m_DefenderInfos[it->OwnerID].Defence.push_back(*it);
		}
	}
	return true;
}

// compares 2 items
bool ItemCompare(const SItem& a, const SItem& b)
{
	if (a.OwnerID < b.OwnerID)
		return true;
	if (a.OwnerID > b.OwnerID)
		return false;

	return a.Type < b.Type;
}


void FillRFTable(RFTYPE rfType)
{
	int i;
	for (i = 0; i < T_END; i++)
	{
		for (int j = 0; j < T_END; j++)
			m_RF[i][j] = 10000;
	}

	switch (rfType)
	{
	case RF_065:
	{
		for (i = 0; i < T_SHIPEND; i++)
		{
			m_RF[i][T_SPIO] = 2000;
			m_RF[i][T_SAT] = 2000;
		}
		m_RF[T_SPIO][T_SPIO] = 10000;
		m_RF[T_SPIO][T_SAT] = 10000;
		m_RF[T_SAT][T_SAT] = 10000;

		m_RF[T_KREUZER][T_LJ] = 3333;
		m_RF[T_KREUZER][T_RAK] = 1000;

		m_RF[T_ZER][T_LL] = 1000;

		m_RF[T_BOMBER][T_LL] = 500;
		m_RF[T_BOMBER][T_RAK] = 500;
		m_RF[T_BOMBER][T_SL] = 1000;
		m_RF[T_BOMBER][T_IONEN] = 1000;

		m_RF[T_TS][T_SPIO] = 8;
		m_RF[T_TS][T_SAT] = 8;
		m_RF[T_TS][T_RAK] = 50;
		m_RF[T_TS][T_LL] = 50;
		m_RF[T_TS][T_LJ] = 50;
		m_RF[T_TS][T_SS] = 333;
		m_RF[T_TS][T_SL] = 100;
		m_RF[T_TS][T_IONEN] = 100;
		m_RF[T_TS][T_SJ] = 100;
		m_RF[T_TS][T_GAUSS] = 200;
		m_RF[T_TS][T_KREUZER] = 303;
		m_RF[T_TS][T_BOMBER] = 400;
		m_RF[T_TS][T_ZER] = 2000;
		m_RF[T_TS][T_KT] = 40;
		m_RF[T_TS][T_GT] = 40;
		m_RF[T_TS][T_REC] = 40;
		m_RF[T_TS][T_KOLO] = 40;

	}
	break;
	case RF_075:
	{
		for (i = 0; i < T_SHIPEND; i++)
		{
			m_RF[i][T_SPIO] = 2000;
			m_RF[i][T_SAT] = 2000;
		}
		m_RF[T_SPIO][T_SPIO] = 10000;
		m_RF[T_SPIO][T_SAT] = 10000;
		m_RF[T_SAT][T_SAT] = 10000;
		m_RF[T_SAT][T_SPIO] = 10000;

		m_RF[T_SJ][T_KT] = 3333;

		m_RF[T_KREUZER][T_LJ] = 1667;
		m_RF[T_KREUZER][T_RAK] = 1000;

		m_RF[T_ZER][T_LL] = 1000;
		m_RF[T_ZER][T_IC] = 5000;

		m_RF[T_BOMBER][T_LL] = 500;
		m_RF[T_BOMBER][T_RAK] = 500;
		m_RF[T_BOMBER][T_SL] = 1000;
		m_RF[T_BOMBER][T_IONEN] = 1000;

		m_RF[T_TS][T_SPIO] = 8;
		m_RF[T_TS][T_SAT] = 8;
		m_RF[T_TS][T_RAK] = 50;
		m_RF[T_TS][T_LL] = 50;
		m_RF[T_TS][T_LJ] = 50;
		m_RF[T_TS][T_SS] = 333;
		m_RF[T_TS][T_SL] = 100;
		m_RF[T_TS][T_IONEN] = 100;
		m_RF[T_TS][T_SJ] = 100;
		m_RF[T_TS][T_GAUSS] = 200;
		m_RF[T_TS][T_KREUZER] = 303;
		m_RF[T_TS][T_BOMBER] = 400;
		m_RF[T_TS][T_ZER] = 2000;
		m_RF[T_TS][T_KT] = 40;
		m_RF[T_TS][T_GT] = 40;
		m_RF[T_TS][T_REC] = 40;
		m_RF[T_TS][T_KOLO] = 40;
		m_RF[T_TS][T_IC] = 667;

		m_RF[T_IC][T_KT] = 3333;
		m_RF[T_IC][T_GT] = 3333;
		m_RF[T_IC][T_SJ] = 2500;
		m_RF[T_IC][T_KREUZER] = 2500;
		m_RF[T_IC][T_SS] = 1429;
	}
	break;
	}
}