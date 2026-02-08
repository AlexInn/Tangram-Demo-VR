# Tangram VR Demo - Setup Tecnico & Configurazione VR

## Piattaforma di Destinazione

* **Hardware:** Meta Quest 2 / Meta Quest 3
* **OS:** Android
* **Architettura:** ARM64

## Versione Unity

* **2022.3.52f3 (LTS)**

## Pacchetti Core & Dipendenze

* **Render Pipeline:** Universal Render Pipeline (URP)
* **XR Framework:** XR Interaction Toolkit (v2.6.5+)
* **Input System:** Unity Input System (Action-based)
* **XR Plugin Management:** OpenXR / Oculus

## Configurazione Rendering (Asset URP)

Impostazioni ottimizzate per VR standalone:

* **HDR:** **Disabilitato** (OFF) - Critico per ottimizzazione memoria e performance.
* **Post-Processing:**
* **Tonemapping:** Abilitato (Modalità: *ACES* su LDR) via Global Volume.
* **Color Adjustments:** Post Exposure / Contrast / Saturation attivi per compensare l'assenza di HDR.

* **Lighting:**
* **Main Light:** Baked (Mixed Lighting).
* **Additional Lights:** Realtime (Spotlights).

* **Shadows (Ombre):**
* **Soft Shadows:** Abilitate.
* **Additional Lights Shadowmap Resolution:** 2048 (per eliminare artefatti/aliasing su luci dinamiche).
* **Shadow Distance:** Ottimizzata per room-scale (15-20m).

## Baking & Lightmapping

* **Lightmap Resolution:** Bassa/Media (Globale).
* **Scale in Lightmap:** Aumentata (2x - 4x) specificamente su Tavoli interattivi/Props per evitare ombre scalettate (aliasing).
* **Filtering:** Advanced.
* **Compression:** High Quality (o *None* se persistono artefatti visivi).

## Setup Interazione XR

Configurazione basata su *Starter Assets* modificati.

### Interaction Layers

* **Teleport Interactor (Ray):** Mask impostata solo su layer `Teleport`.
* **Teleport Area/Anchor:** Layer impostato su `Teleport`.
* **Physics Ray:** Mask impostata su `Everything` (escluso Teleport).

### Schema di Locomozione

Gestione input separata per evitare conflitti:

* **Left Controller:**
* *Move:* Abilitato (Continuous Move Provider).
* *Turn:* Disabilitato.
* *Teleport:* Gestito via script custom.

* **Right Controller:**
* *Move:* Disabilitato.
* *Turn:* Abilitato (Snap Turn Provider).
* *Teleport:* Gestito via script custom.

## Architettura Software & Scripting Custom

Il sistema si basa su tre pilastri: Logica di Gioco, Tracciamento Utente e Data Logging.

### 1. Core Logic & Game Manager

* **`TangramPatternMatcher.cs`:**
* **Funzione:** Verifica il completamento del puzzle.
* **Logica Relativa:** Calcola posizione/rotazione dei pezzi rispetto a un pezzo "Anchor" (Capo), permettendo la risoluzione ovunque nello spazio.
* **Win Condition (Strict):** La vittoria scatta solo se i pezzi sono posizionati correttamente E **tutti i pezzi sono stati rilasciati** (incluso l'Anchor).
* **Eventi:** Invoca `OnWin` (Audio, FX, Stop Logging).

* **`TeleportToggler.cs`:**
* Toggle runtime per abilitare/disabilitare il Teleport Interactor.
* Forza la separazione rigida dei ruoli joystick (Move vs Turn) all'inizializzazione.
* Disabilita la logica standard `ActionBasedControllerManager`.

### 2. Data Logging System

* **`TangramLogger.cs`:**
* **Funzione:** Centralizza la raccolta dati e la scrittura su file CSV.
* **Gestione Sessioni:** Aggiunge automaticamente un header e una riga vuota se il file esiste già (append mode).
* **Durata Azioni:**
* *GRAB:* Calcola la durata (DeltaTime) tra `SelectEntered` e `SelectExited` usando un Dictionary interno.
* *GAZE:* Riceve la durata dello sguardo dal Tracker.
* **Safety Switch:** Disabilita qualsiasi scrittura dopo l'evento di Vittoria (`FINE`).
* **Output Path:** `Application.persistentDataPath/TangramLog.csv`.

### 3. User Tracking (Gaze)

* **`HeadGazeTracker.cs` (Main Camera):**
* **Funzione:** Raycasting continuo dal centro degli occhi.
* **Logica:** Rileva oggetti con script `InterestZone`. Calcola il tempo di permanenza dello sguardo su una zona specifica.
* **Output:** Invia i dati al Logger solo al cambio di zona o distoglimento dello sguardo.

* **`InterestZone.cs`:**
* Componente "etichetta" da assegnare agli oggetti di interesse (Muri, Tavoli, UI). Richiede Collider.

### 4. Feedback Visivo (Reward)

* **`DecalChanger.cs`:**
* **Funzione:** Sostituisce la texture di un *URP Decal Projector* alla vittoria.
* **Fix Shader URP:** Gestisce correttamente i canali colore (evitando che il Rosso venga interpretato come Alpha) e resetta la tinta del materiale a Bianco puro.

## Struttura Dati (CSV Output)

Il file di log utilizza il punto e virgola (`;`) come separatore per compatibilità Excel/Sheets.

| Colonna | Descrizione | Esempio |
| :--- | :--- | :--- |
| **Date** | Data sessione (dd/MM/yyyy) | `13/01/2026` |
| **Time** | Ora evento (fine azione) | `10:45:01` |
| **Event** | Tipo evento (`GRAB`, `GAZE`, `FINE`) | `GRAB` |
| **ObjectName** | Nome oggetto o Zona interesse | `Triangolo_Rosso` |
| **Duration** | Durata in secondi (2 decimali) | `4.52` |

## Note di Sviluppo

* **Legacy VR:** Rimossi script obsoleti basati su `XRSettings.enabled` e tutte le librerie e riferimenti a SteamVR.
* **Assembly Definitions:** Eliminato file `.asmdef` dagli *Starter Assets* per consentire l'accesso agli script interni (`ActionBasedControllerManager`) dal codice utente.
