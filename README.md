
## Abgabe-Links

- GitHub Repository: https://github.com/erayak-uni/Games-Abgaben
- Video: Auf Youtube

## ProBuilder-Level

Das Level wurde als Arena mit Innen- und Außenbereichen aufgebaut. Es enthält mehrere Ebenen, Brücken, Plattformen, Wandbereiche, Deckungselemente, Trampoline und Grapple-Punkte. Dies wurde mit Hilfe von Pro Builder Elementen umgesetzt

## Player Movement

Der Spieler besitzt einen First-Person-Controller mit erweiterten Bewegungsfunktionen:

- normales Laufen mit WASD
- Maussteuerung zum Umschauen
- Double Jump
- Dash
- Spiderman Netze
- Trampoline, die den Spieler nach oben katapultieren
- Wall climb

## Kampf-System

Der Spieler kann mit der linken Maustaste schießen. Treffer verursachen Schaden an Bots. Zusätzlich kann der Spieler Bomben werfen, die nach kurzer Zeit explodieren und Flächenschaden verursachen.

## Bot-System

Die Bots verwenden NavMesh zur Navigation. Sie erkennen den Spieler in einem bestimmten Bereich und greifen bei Sichtkontakt mit Projektilen an. Bots besitzen Lebenspunkte, können Schaden nehmen, sterben und nach einer Zeit respawnen sie wieder

## Animationen

Für die Bots wurde eine einfache Animationslogik umgesetzt. Sie zeigt grundlegende Zustände wie Idle, Laufen, Angriff, Treffer und Sterben. Dadurch sind die Bot-Zustände im Spiel visuell erkennbar
