import subprocess

class ExternalInterface():
    
    def __init__(self, *args, **kwargs):
        input("Press Enter to close...")
        return super().__init__(*args, **kwargs)

    def start_sim(self):
        subprocess.call('speedsim.exe')

    def write_data_file(self, attacker, defender, number_of_sims):
        with open("data.txt", 'w') as f:
            f.write(",".join(map(str,attacker)) + '\n')
            f.write(",".join(map(str,defender)) + '\n')
            f.write(str(number_of_sims))


interface = ExternalInterface()
attacker_list = [0,0,1000,300,100,50,0,0,0,10,0,5,0,20]
defender_list = [0,0,0,0,0,0,0,0,0,0,0,0,0,0,1000,800,200,40,140,4,1,1]
interface.write_data_file(attacker_list,defender_list,20)
interface.start_sim()