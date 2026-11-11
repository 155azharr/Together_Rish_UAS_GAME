# TOGETHERISH: MIND THE GAP

> Platformer 2D berbasis Unity dengan tiga level, mekanik long jump, air dash, checkpoint, hazard, dan progres misi yang dibangun untuk kebutuhan proyek UAS game.

![TOGETHERISH - Main Menu](Assets/Backgrounds/TOGETHERISH_MainMenu_4K.png)

## Tentang Project

**TOGETHERISH: MIND THE GAP** adalah game platformer 2D bertema petualangan antar dunia. Pemain mengendalikan karakter **Ninja Girl** dalam misi mengumpulkan dua item utama, membuka kemampuan baru, melewati jebakan, dan mencapai gerbang keluar pada setiap level.

Game ini dirancang sebagai prototype edukasi/akademik dengan integrasi sistem menu, progres level, hazard, checkpoint, HUD, dan level select. Selain itu, repository juga mencantumkan komponen jaringan awal berbasis Unity Netcode (`NetworkCatPlayer.cs`) sebagai fondasi untuk pengembangan online di masa depan, namun fitur multiplayer aktif belum menjadi bagian utama gameplay saat ini.

## Fitur Utama

- Main Menu dengan pilihan Play, Level Select, Options, dan Quit
- Level Select untuk Level 1 sampai Level 3
- Platformer 2D dengan gerak kiri/kanan, lompat dasar, long jump, dan air dash
- Sistem checkpoint dan respawn otomatis
- Koleksi item misi: Kunci Kristal dan Relik Es
- Hazard berupa duri, jatuh dari map, platform berbahaya, serta patrol hazard
- HUD permainan yang menampilkan level, checkpoint, item, dan charge skill
- Pause menu dan layar kemenangan
- Tema visual berbeda untuk setiap level: ice valley, dune ruins, mushroom citadel

## Gameplay Loop

1. Pemain memulai dari menu utama.
2. Pilih level atau langsung masuk Level 1.
3. Kumpulkan Kunci Kristal dan Relik Es.
4. Gunakan long jump dan air dash untuk melewati celah, duri, platform, dan jebakan.
5. Jika terkena hazard, pemain akan respawn ke checkpoint terakhir.
6. Setelah item misi lengkap, pemain mencapai EXIT untuk menyelesaikan level.

## Kontrol

| Aksi | Tombol |
| --- | --- |
| Bergerak kiri | A atau Left Arrow |
| Bergerak kanan | D atau Right Arrow |
| Lompat normal | Space, W, atau Up Arrow |
| Long Jump | Tahan Shift lalu tekan tombol lompat di tanah |
| Air Dash | Tahan Shift lalu tekan tombol lompat di udara |
| Pause / resume | Escape atau P |
| Restart level | R |
| Kembali ke menu | M |
| Level berikutnya | N |

Catatan: Long Jump terbuka setelah item pertama didapatkan dan memiliki 3 charge. Air Dash terbuka setelah item kedua didapatkan dan hanya dapat digunakan sekali tiap kali melompat.

## Persyaratan

- Unity Hub
- Unity Editor versi 6.6.3f1
- Sistem operasi yang mendukung Unity

## Cara Menjalankan

1. Clone atau unduh repository ini.
2. Buka Unity Hub.
3. Pilih Add project from disk.
4. Arahkan ke folder project `Project Game UAS`.
5. Buka project menggunakan Unity Editor yang kompatibel.
6. Tunggu import asset dan kompilasi selesai.
7. Buka scene `Assets/Main Menu.unity` jika belum terbuka.
8. Tekan tombol Play untuk menjalankan game.
9. Pilih PLAY untuk mulai Level 1 atau pilih LEVEL SELECT untuk memilih level tertentu.

> Penting: menu UI dibuat agar bisa berjalan langsung saat project dibuka di Unity. Jika Game View terlihat kosong saat pertama kali, biasanya itu berarti menunggu input dari menu atau scene utama belum dimuat.

## Scene Utama

- `Assets/Main Menu.unity` — menu utama
- `Assets/Scenes/SampleScene.unity` — Level 1 (Icebound)
- `Assets/Scenes/Level2.unity` — Level 2 (Dune Runner)
- `Assets/Scenes/Level3.unity` — Level 3 (Moonlit Mushroom Citadel)

## Struktur Project

```text
Project Game UAS/
├── Assets/
│   ├── Backgrounds/
│   ├── Editor/
│   ├── Scenes/
│   ├── Scripts/
│   ├── Welcome/
│   └── ...
├── Packages/
├── ProjectSettings/
├── README.md
├── SRS.md
├── Project Game UAS.slnx
├── .gitignore
├── .gitattributes
└── UserSettings/
```

## Teknologi yang Digunakan

| Teknologi | Kegunaan |
| --- | --- |
| Unity 6.6.3f1 | Game engine utama |
| C# | Bahasa pemrograman |
| Unity Input System | Input keyboard dan kontrol gameplay |
| Unity 2D Physics | Collider, rigidbody, platform, dan hazard |
| Unity Netcode | Pondasi untuk pengembangan fitur online/multiplayer |

## Status Project

Project saat ini berada dalam status prototype/fase pengembangan awal dengan fokus pada gameplay single-player dan mekanik platformer. Fitur online, lobby, matchmaking, serta integrasi server/relay belum menjadi kebutuhan utama dari baseline saat ini.

## Dokumentasi

- [SRS.md](SRS.md) — Software Requirements Specification

## Informasi Project

- Nama: TOGETHERISH: MIND THE GAP
- Genre: 2D Platformer
- Tema: Petualangan tiga dunia
- Status: Prototype
- Platform: PC / Laptop
- Mode: Single-player (prototype), dengan potensi pengembangan multiplayer di masa depan

---

Dikembangkan untuk kebutuhan tugas UAS game development.
