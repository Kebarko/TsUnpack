# TsUnpack

## Popis programu

Program slúži na rozbaľovanie aktivít vo formáte APK. Cieľom programu je nahradiť pôvodný tsunpack so zachovaním jeho funkcionality a priniesť niekoľko vylepšení.
Hlavný rozdiel voči pôvodnému programu je ten, že umožňuje zvoliť miesto kde sa rozbalí aktivita. Spustením súboru tsunpack.exe sa zobrazí konfiguračné okno programu. Ak niekto používa viacero inštalácií MSTS môže si zvoliť do ktorej inštalácie sa majú aktivity rozbaľovať.

![Image](art/config.png)

Možnosť „Default path“ znamená, že aktivita sa rozbalí do adresára, ktorý je zapísaný v registroch po inštalácii MSTS.
Možnosť „Custom path“ znamená, že aktivita sa rozbalí do adresára, ktorý si užívateľ zvolí. Tento adresár musí obsahovať podadresár Routes s príslušnou traťou.
Program na rozdiel od pôvodného zachováva veľkosť písmen v názvoch vytváraných súborov.
Ak pri rozbaľovaní aktivity nájde existujúce súbory, tak sa nepýta pre každý súbor zvlášť či sa má prepísať. Zobrazí okno v ktorom si užívateľ môže vybrať ktoré súbory chce prepísať.

![Image](art/overwrite.png)

Po skončení rozbalenia aktivity program zobrazí okno s výsledkom rozbalenia. Okno obsahuje zoznam súborov – novo vytvorených, prepísaných, preskočených a chybných.

![Image](art/result.png)

## Inštalácia

Inštalácia programu je jednoduchá. Stačí v adresári Utils nahradiť pôvodný súbor tsunpack.exe novým. Kto chce pôvodný súbor si môže zazálohovať.

Po inštalácii MSTS sa v registroch nachádza chyba, ktorá znemožňuje rozbaľovanie aktivít vo formáte APK. Táto chyba sa dá odstrániť úpravou v registroch. Je potrebné nájsť jeden z týchto dvoch kľúčov (druhý kľúč sa opraví automaticky):
`HKEY_LOCAL_MACHINE\SOFTWARE\Classes\TrainSim.Packaged.Activity\shell\open\command`

`HKEY_CLASSES_ROOT\TrainSim.Packaged.Activity\shell\open\command`

Tu sa nachádza chybný zápis:
`"E:\Games\TrainSim\Utils\TSUnpack.exe" " "`, stačí ak sa do poslednej medzery vloží `%1`.

Správny zápis teda je:
`"E:\Games\TrainSim\Utils\TSUnpack.exe" "%1"`

Cesta k súboru TsUnpack.exe je individuálna.
