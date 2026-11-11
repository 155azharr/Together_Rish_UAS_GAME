# Software Requirements Specification (SRS)

## TOGETHERISH: MIND THE GAP

| Atribut | Nilai |
| --- | --- |
| Versi dokumen | 2.0 |
| Status | Prototype multi-level platformer |
| Tanggal | 11 November 2026 |
| Engine | Unity 6.6.3f1 |
| Platform utama | PC / Laptop |
| Mode gameplay baseline | Single-player |

## 1. Pendahuluan

### 1.1 Tujuan

Dokumen ini mendefinisikan kebutuhan fungsional dan nonfungsional untuk game **TOGETHERISH: MIND THE GAP**. Tujuan utama dokumen adalah memastikan bahwa gameplay, menu, mekanik, progres level, hazard, checkpoint, dan pengalaman pemain dapat dipahami, dikembangkan, dan diuji secara konsisten.

### 1.2 Cakupan Produk

Produk adalah game platformer 2D yang menampilkan tiga level dengan tema berbeda: Icebound, Dune Runner, dan Moonlit Mushroom Citadel. Pemain mengendalikan karakter Ninja Girl untuk mengumpulkan item utama yang berbeda per level—Level 1 menggunakan Kunci Kristal dan Relik Es, Level 2 menggunakan Sigil Gurun dan Relik Pasir, serta Level 3 menggunakan Moonstone dan Relik Bulan—sebelum membuka kemampuan Long Jump dan Air Dash, menghindari jebakan, dan mencapai gerbang EXIT di tiap level.

Project ini juga memuat sampel jaringan Unity Netcode (`NetworkCatPlayer.cs`) sebagai fondasi pengembangan multiplayer masa depan, namun fitur online tidak ditetapkan sebagai kebutuhan utama dari baseline saat ini.

### 1.3 Definisi Istilah

| Istilah | Definisi |
| --- | --- |
| Checkpoint | Titik respawn aktif yang menyimpan posisi pemain saat melewati area tertentu. |
| Relic | Item utama yang harus dikumpulkan untuk menyelesaikan level. |
| Long Jump | Lompatan khusus dengan charge dan gaya dorong horizontal/vertikal yang lebih besar. |
| Air Dash | Teknik bergerak cepat di udara yang hanya bisa dilakukan setelah skill terbuka. |
| Charge | Satu unit daya yang digunakan untuk long jump. |
| Hazard | Objek berbahaya seperti duri, fall reset, atau patrol hazard. |
| EXIT | Area finish yang hanya bisa dipakai bila semua misi utama selesai. |
| HUD | Informasi visual gameplay seperti level, checkpoint, item, dan sisa charge. |

### 1.4 Referensi

- Unity Editor 6.6.3f1
- `README.md`
- `Assets/Scripts/NinjaGirlController.cs`
- `Assets/Scripts/GameProgressManager.cs`
- `Assets/Scripts/Level1MainMenu.cs`
- `Assets/Scripts/NetworkCatPlayer.cs`

## 2. Deskripsi Umum

### 2.1 Perspektif Produk

Game dirancang dalam Unity 2D dengan kombinasi `Rigidbody2D`, `Collider2D`, animator, sistem input, dan scene-based flow. Produknya terdiri dari menu utama, level select, mekanik platforming, sesi gameplay, dan layar akhir mission.

### 2.2 Fungsi Produk

- Menampilkan menu utama sebelum permainan dimulai
- Memuat level tertentu melalui Level Select atau tombol Play
- Mengendalikan karakter dengan input keyboard
- Menjalankan mekanik loncat dan lompatan khusus
- Mengumpulkan item misi dan membuka skill baru
- Menyimpan respawn location berdasarkan checkpoint aktif
- Mengembalikan pemain ke posisi terakhir saat jatuh atau terkena hazard
- Mengizinkan pemain menyelesaikan level hanya setelah misi utama lengkap
- Menampilkan splash, HUD, pause menu, dan victory screen

### 2.3 Kelas Pengguna

- Player: mengoperasikan game secara langsung melalui keyboard
- Developer: mengatur scene, script, gameplay balancing, dan testing di Unity Editor
- Penguji: memeriksa fitur gameplay sesuai acceptance criteria

### 2.4 Lingkungan Operasi

| Komponen | Baseline |
| --- | --- |
| Engine | Unity 6.6.3f1 |
| Input | Unity Input System |
| Rendering | 2D Sprite/URP-oriented gameplay |
| Physics | Unity 2D Physics |
| Target platform | PC/Laptop |
| Input device | Keyboard |

### 2.5 Batasan

- Gameplay baseline bersifat single-player
- Fitur multiplayer, lobby, matchmaking, relay, dan server online tidak menjadi target utama
- Progres checkpoint berlaku selama sesi permainan dan tidak dibangun sebagai save system lintas sesi
- Project saat ini fokus pada prototype mekanik dan gameplay loop, bukan game final produksi

### 2.6 Asumsi dan Dependensi

- Project dibuka di Unity dengan dependency yang terdaftar dalam `Packages` dan `ProjectSettings`
- Scene utama tersedia dan tetap terdaftar di Build Settings
- Asset sprite, background, tile dan prefab yang dibutuhkan tersedia di `Assets`
- Level dan trigger gameplay telah diberikan script dan collider yang kompatibel

## 3. Antarmuka Eksternal

### 3.1 Antarmuka Pengguna

- Main Menu menampilkan tombol PLAY, LEVEL SELECT, OPTIONS, dan QUIT
- Level Select menampilkan daftar level yang tersedia
- Options menampilkan volume, pengaturan resolusi, dan informasi kontrol
- HUD menampilkan status stage, item, checkpoint, dan charge skill
- Popup menampilkan notifikasi skill dan status misi
- Victory Screen menampilkan hasil level dan opsi lanjut/ulang/menu

### 3.2 Antarmuka Perangkat

Input utama adalah keyboard.

| Aksi | Input |
| --- | --- |
| Gerak kiri/kanan | A/D atau panah kiri/kanan |
| Lompat | Space/W/Up Arrow |
| Long Jump | Shift + lompat |
| Air Dash | Shift + lompat di udara |
| Pause | Escape atau P |
| Restart | R |
| Kembali ke menu | M |
| Lanjut level | N |

### 3.3 Antarmuka Perangkat Lunak

- Unity SceneManager memuat scene berdasarkan build settings
- Unity Input System membaca input keyboard
- Unity 2D Physics menangani collider, collision, dan trigger hazard
- PlayerPrefs digunakan untuk menyimpan pengaturan volume/menu

## 4. Kebutuhan Fungsional

Prioritas: P1 = wajib, P2 = penting, P3 = bisa ditunda.

### 4.1 Main Menu dan Navigasi

- FR-001 (P1): Sistem harus menampilkan Main Menu saat project dijalankan.
- FR-002 (P1): Tombol PLAY harus memuat level gameplay utama.
- FR-003 (P2): Level Select harus menampilkan daftar level yang tersedia.
- FR-004 (P2): Menu Options harus memuat pengaturan volume dan tampilan dasar.
- FR-005 (P2): Pengaturan audio dasar harus disimpan melalui PlayerPrefs saat pemain kembali ke menu.
- FR-006 (P1): Tombol QUIT harus menutup aplikasi atau menghentikan Play Mode saat di Unity Editor.

### 4.2 Gerak Karakter

- FR-007 (P1): Pemain dapat bergerak ke kiri dan kanan dengan input keyboard.
- FR-008 (P1): Pemain dapat melakukan lompat normal saat menyentuh tanah.
- FR-009 (P1): Long Jump hanya dapat dilakukan setelah skill terbuka dan charge tersedia.
- FR-010 (P2): Air Dash hanya dapat dilakukan di udara dan setelah skill terbuka.
- FR-011 (P1): Karakter harus dapat mengalihkan orientasi sprite mengikuti arah gerak.

### 4.3 Checkpoint dan Kumpulan Item

- FR-012 (P1): Saat pemain menyentuh checkpoint, sistem menyimpan lokasi respawn terbaru.
- FR-013 (P1): Pengambilan item pertama membuka skill Long Jump dan mengaktifkan charge level tertentu.
- FR-014 (P1): Pengambilan item kedua membuka skill Air Dash dan memperbarui kemampuan karakter.
- FR-015 (P1): Setiap checkpoint atau item harus tidak bisa diulang secara berlebihan tanpa konsekuensi yang tidak diharapkan.
- FR-016 (P2): HUD harus menampilkan status item utama dan jumlah charge yang tersisa.

### 4.4 Hazard, Respawn, dan Finish

- FR-017 (P1): Hazard seperti duri dan fall reset harus memicu respawn ke checkpoint terakhir.
- FR-018 (P1): Setelah respawn, progres item dan skill yang sudah didapat harus tetap tersimpan dalam sesi berjalan.
- FR-019 (P1): Player tidak boleh menyelesaikan level sebelum item utama level yang aktif terkumpul, dengan variasi nama item sesuai level: Level 1 (Kunci Kristal dan Relik Es), Level 2 (Sigil Gurun dan Relik Pasir), Level 3 (Moonstone dan Relik Bulan).
- FR-020 (P1): Saat menyentuh EXIT dengan misi lengkap, game menampilkan kondisi victory.
- FR-021 (P1): Setelah menang, pemain dapat memilih ulang level atau kembali ke menu utama.
- FR-022 (P2): Sistem harus menampilkan notifikasi yang menjelaskan status misi dan skill baru.

### 4.5 Level dan Progression

- FR-023 (P1): Setiap level memiliki layout, hazard, dan tujuan misi yang tetap konsisten.
- FR-024 (P2): Level 1 dapat dimulai langsung dari menu, lalu level berikutnya dapat dibuka melalui level select.
- FR-025 (P2): Sistem harus mendukung transisi level sesuai tombol aksi lanjut level setelah victory.
- FR-026 (P1): Restart level harus memulai ulang progres level dari initial spawn atau checkpoint aktif terakhir.

## 5. Kebutuhan Non-Fungsional

- NFR-001 (P1): Antarmuka game harus mudah dipahami dan memiliki kontras visual yang cukup.
- NFR-002 (P2): Menu dan HUD harus responsif terhadap ukuran layar dan tetap rapi di viewport target.
- NFR-003 (P2): Target performa game utama berkisar 60 FPS pada perangkat pengembangan.
- NFR-004 (P1): Sistem input harus konsisten dan menghindari double-trigger saat tombol input ditekan.
- NFR-005 (P2): Script gameplay harus dipisah berdasarkan fungsi (menu, player control, progress, hazard, finish).
- NFR-006 (P2): Artworks dan UI harus menjaga kualitas visual yang cocok dengan tema game.
- NFR-007 (P2): Project harus dapat dibuka dan dijalankan di Unity Editor dengan dependency yang tersedia.

## 6. Model Data dan Status

| Data | Tipe | Deskripsi |
| --- | --- | --- |
| `initialSpawn` | Vector3 | Posisi awal pemain saat scene dimuat. |
| `respawnPosition` | Vector3 | Posisi checkpoint aktif terakhir. |
| `activeCheckpoint` | Integer | Indeks checkpoint yang sedang aktif. |
| `hasCrystalKey` | Boolean | Status Kunci Kristal telah dikumpulkan. |
| `hasIceRelic` | Boolean | Status Relik Es telah dikumpulkan. |
| `jumpUpgradeLevel` | Integer | Level kemampuan loncat khusus. |
| `longJumpCharges` | Integer | Jumlah charge yang dimiliki saat ini. |
| `masterVolume` | Float | Nilai volume master yang disimpan di PlayerPrefs. |
| `paused` | Boolean | Status menu pause aktif atau tidak. |
| `levelComplete` | Boolean | Status level sudah selesai atau belum. |

## 7. Use Case Utama

### UC-01: Mulai permainan

1. Pemain menjalankan game.
2. Sistem menampilkan Main Menu.
3. Pemain memilih PLAY atau Level Select.
4. Sistem memuat scene level yang dipilih.
5. Pemain masuk ke tahap gameplay.

### UC-02: Mengumpulkan item

1. Pemain bergerak ke area item misi.
2. Sistem mengaktifkan event pickup.
3. Item muncul sebagai collectible dan status misi berubah.
4. Skill baru terbuka sesuai item yang didapat.

### UC-03: Menggunakan Long Jump

1. Pemain menahan Shift.
2. Pemain menekan tombol lompat saat berada di tanah.
3. Sistem membatasi long jump berdasarkan charge dan progression.
4. Karakter bergerak dengan gaya dorong yang lebih besar.

### UC-04: Menghadapi hazard

1. Pemain menyentuh duri atau jatuh dari map.
2. Sistem memberi notifikasi kematian/respawn.
3. Karakter dipindahkan ke checkpoint terakhir.
4. Progres item dan skill tetap dipertahankan.

### UC-05: Menyelesaikan level

1. Pemain mengumpulkan semua item misi utama.
2. Pemain mencapai EXIT.
3. Sistem menghentikan pergerakan dan menampilkan victory screen.
4. Pemain memilih ulang level atau kembali ke menu.

## 8. Kriteria Penerimaan

| ID | Skenario | Hasil yang diharapkan |
| --- | --- | --- |
| AT-001 | Menjalankan project dari Unity | Main Menu muncul dengan tombol utama. |
| AT-002 | Memilih PLAY | Level gameplay dimuat dan pemain muncul di spawn awal. |
| AT-003 | Tekan tombol lompat saat grounded | Lompat normal terjadi. |
| AT-004 | Menahan Shift lalu menekan lompat setelah skill terbuka | Long Jump aktif dan charge berkurang. |
| AT-005 | Menekan lompat di udara setelah skill terbuka | Air Dash aktif sesuai aturan. |
| AT-006 | Menyentuh checkpoint | Posisi respawn diperbarui dan skill baru terbuka jika sesuai. |
| AT-007 | Menyentuh hazard | Pemain respawn ke checkpoint terakhir. |
| AT-008 | Kumpulkan item utama | Status misi dan skill berubah. |
| AT-009 | Sentuh EXIT sebelum item lengkap | Level tidak selesai, notifikasi muncul. |
| AT-010 | Sentuh EXIT setelah item lengkap | Victory screen tampil. |
| AT-011 | Pilih RESTART | Level dimulai ulang sesuai flow yang ditentukan. |
| AT-012 | Pilih MENU UTAMA | Return ke Main Menu. |
| AT-013 | Ubah volume melalui menu | Pengaturan tersimpan dan diterapkan. |
| AT-014 | Buka LEVEL SELECT | Daftar level terlihat dan dapat dibuka. |

## 9. Pemetaan Kebutuhan ke Implementasi

| Area | Implementasi utama |
| --- | --- |
| Menu utama | `Assets/Scripts/Level1MainMenu.cs` |
| Kontrol karakter | `Assets/Scripts/NinjaGirlController.cs` |
| Progres level dan HUD | `Assets/Scripts/GameProgressManager.cs` |
| Checkpoint dan item | `Assets/Scripts/CheckpointTrigger.cs` |
| Hazard dan finish | `Assets/Scripts/HazardTrigger.cs`, `Assets/Scripts/FinishTrigger.cs` |
| Jaringan awal | `Assets/Scripts/NetworkCatPlayer.cs` |
| Scene utama | `Assets/Main Menu.unity` |
| Scene level | `Assets/Scenes/SampleScene.unity`, `Assets/Scenes/Level2.unity`, `Assets/Scenes/Level3.unity` |

## 10. Batasan dan Perluasan Masa Depan

- Baseline saat ini belum mengadopsi multiplayer aktif sebagai kebutuhan utama.
- Fitur matchmaking, lobby, server relay, networking synchronization, dan save system lintas sesi dapat dikembangkan pada iterasi berikutnya.
- Game saat ini fokus pada gameplay prototype dan validasi mekanik platformer.
- Langkah pengembangan selanjutnya dapat mencakup fitur audio, polish visual, balancing level, dan pengujian build final.
