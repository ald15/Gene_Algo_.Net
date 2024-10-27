import random as r

FILE_NAME = "world.txt"
N = 10
MI, MA = 1, 100

with open(FILE_NAME, "w+") as f:
    for j in range(N):
        row = ','.join([str(r.randint(MI, MA)) for i in range(N)])
        f.write("{" + row + '},\n')