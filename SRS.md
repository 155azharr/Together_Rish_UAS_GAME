# Software Requirements Specification (SRS)

## TOGETHERISH : MIND THE GAP

| Atribut               | Nilai                      |
| --------------------- | -------------------------- |
| Versi dokumen         | 1.0                        |
| Status                | Baseline prototype Level 1 |
| Tanggal               | 14 Oktober 2026            |
| Engine                | Unity 6.6.3f1              |
| Platform pengembangan | Windows                    |

## 1. Pendahuluan

### 1.1 Tujuan

Dokumen ini mendefinisikan kebutuhan fungsional dan nonfungsional untuk prototype game **TOGETHERISH : MIND THE GAP**. Dokumen ditujukan bagi pengembang, penguji, pembimbing, dan pemangku kepentingan presentasi agar cakupan, perilaku, batasan, dan kriteria penerimaan game dapat diperiksa secara konsisten.

### 1.2 Cakupan Produk

Produk adalah game platformer 2D single-player dengan satu level bertema lembah es. Pemain mengendalikan Ninja Girl, mengumpulkan Kunci Kristal dan Relik Es, memperoleh peningkatan long jump pada checkpoint, menghindari duri, dan mencapai EXIT. Main menu menyediakan navigasi ke level, pengaturan dasar, dan keluar.

### 1.3 Istilah

| Istilah    | Definisi                                                                                                 |
| ---------- | -------------------------------------------------------------------------------------------------------- |
| Checkpoint | Pickup yang menyimpan titik respawn dan memberi peningkatan long jump.                                   |
| Relic      | Item misi yang wajib dikumpulkan untuk menyelesaikan Level 1.                                            |
| Long jump  | Lompatan khusus yang menggunakan satu charge dan memberi dorongan vertikal serta horizontal lebih besar. |
| Charge     | Satu pemakaian long jump; pemain memperoleh tiga charge pada setiap checkpoint baru.                     |
| EXIT       | Trigger finish di ujung level.                                                                           |
| HUD        | Informasi checkpoint, relic, dan charge yang ditampilkan selama gameplay.                                |

### 1.4 Referensi

- Unity Editor 6000.6.3f1.
- Package manifest project: `Packages/manifest.json`.
- Scene menu: `Assets/Main Menu.unity`.
- Scene Level 1: `Assets/Scenes/SampleScene.unity`.
- README project: `README.md`.

## 2. Deskripsi Umum

### 2.1 Perspektif Produk

Game dibangun sebagai project Unity 2D menggunakan Universal Render Pipeline 2D, Unity Input System, SpriteRenderer, Animator, Rigidbody2D, Collider2D, dan Tilemap. Urutan scene build adalah Main Menu lalu SampleScene.

### 2.2 Fungsi Produk

- Menampilkan menu awal sebelum Level 1.
- Memulai Level 1 dari spawn kiri.
- Menggerakkan dan melompatkan karakter.
- Menyimpan progress checkpoint selama sesi permainan.
- Mengumpulkan dua relic serta membuka long jump bertingkat.
- Mengembalikan pemain ke checkpoint saat terkena hazard.
- Menolak penyelesaian level sebelum dua relic diperoleh.
- Menampilkan HUD, popup skill, layar kemenangan, dan pilihan replay/menu.

### 2.3 Kelas Pengguna

- **Pemain:** menggunakan keyboard dan membaca HUD, popup, serta menu.
- **Pengembang/penguji:** membuka scene di Unity, menjalankan Play Mode, dan memeriksa komponen serta Build Settings.

### 2.4 Lingkungan Operasi

| Komponen       | Baseline                                                                                            |
| -------------- | --------------------------------------------------------------------------------------------------- |
| Engine         | Unity 6.6.3f1                                                                                       |
| Input          | Unity Input System                                                                                  |
| Rendering      | Universal Render Pipeline 2D                                                                        |
| Gameplay       | Unity 2D Physics dan Tilemap                                                                        |
| Resolusi menu  | Dirancang responsif; Options menyediakan 1280x720 dan 1920x1080                                     |
| Platform build | Belum dibatasi pada satu target final; diverifikasi melalui Unity Build Profiles sebelum distribusi |

### 2.5 Batasan

- Cakupan gameplay aktif adalah satu level.
- Progress checkpoint dan relic berlaku selama sesi; penyimpanan antar-sesi tidak disediakan.
- Multiplayer, lobby, dan relay tidak termasuk dalam alur Level 1 yang aktif, meskipun beberapa package networking terdapat di project.
- Kontrol yang didokumentasikan adalah keyboard; dukungan gamepad/touch belum ditetapkan.

### 2.6 Asumsi dan Dependensi

- Unity Package Manager dapat menyelesaikan package yang dicantumkan di manifest.
- Scene, sprite, tilemap, dan prefab dalam folder Assets tersedia dan tidak dipindahkan tanpa memperbarui referensi.
- Main Menu dan SampleScene tetap terdaftar di Build Settings.
- Pemain dan trigger menggunakan komponen collider/rigidbody yang kompatibel dengan callback 2D.

## 3. Antarmuka Eksternal

### 3.1 Antarmuka Pengguna

- Main Menu menampilkan PLAY, LEVEL SELECT, OPTIONS, dan QUIT.
- Level Select menyediakan satu level aktif: Level 01 - Icebound.
- Options menyediakan volume utama, fullscreen, pilihan resolusi, dan informasi kontrol.
- HUD Level 1 menunjukkan checkpoint, status Kunci Kristal, status Relik Es, dan jumlah charge long jump.
- Popup menampilkan misi awal, skill baru, kematian/respawn, dan status checkpoint.
- Layar kemenangan menampilkan ucapan selamat, dua relic, skill, tombol MAIN LAGI, dan MENU UTAMA.

### 3.2 Antarmuka Perangkat

Input utama adalah keyboard:

| Aksi                             | Input                                          |
| -------------------------------- | ---------------------------------------------- |
| Gerak kiri/kanan                 | A/D atau panah kiri/kanan                      |
| Lompatan normal                  | Space, W, atau panah atas                      |
| Long jump                        | Tahan Shift kiri/kanan dan tekan tombol lompat |
| Ulangi level setelah menang      | R                                              |
| Kembali ke menu setelah menang   | M                                              |
| Keluar dari halaman Options/menu | Escape                                         |

### 3.3 Antarmuka Perangkat Lunak

- Unity SceneManager memuat scene berdasarkan nama/index Build Settings.
- Unity Input System menyediakan pembacaan keyboard.
- Unity 2D Physics memicu checkpoint, hazard, fall-reset, dan finish.
- PlayerPrefs menyimpan volume utama menu.

## 4. Kebutuhan Fungsional

Prioritas: **P1** wajib untuk baseline; **P2** penting untuk presentasi; **P3** dapat ditunda.

### 4.1 Main Menu

- **FR-001 (P1):** Sistem harus menampilkan Main Menu sebagai scene pertama dalam Build Settings.
- **FR-002 (P1):** Memilih PLAY harus memuat scene SampleScene.
- **FR-003 (P2):** LEVEL SELECT harus menampilkan Level 01 sebagai level aktif dan dapat dimulai.
- **FR-004 (P2):** OPTIONS harus memungkinkan pemain mengubah volume utama, fullscreen, resolusi yang tersedia, dan melihat kontrol.
- **FR-005 (P2):** Sistem harus menyimpan volume utama melalui PlayerPrefs saat pemain memilih SAVE & BACK.
- **FR-006 (P1):** QUIT harus menghentikan aplikasi; di Unity Editor, aksi tersebut menghentikan Play Mode.

### 4.2 Gerak dan Kamera

- **FR-007 (P1):** Pemain harus dapat bergerak kiri/kanan dengan A/D atau tombol panah.
- **FR-008 (P1):** Pemain harus dapat melakukan lompatan normal dengan Space, W, atau panah atas saat menyentuh ground.
- **FR-009 (P2):** Kamera utama harus mengikuti pemain dengan smoothing sambil mempertahankan posisi Z kamera.
- **FR-010 (P1):** Setiap scene gameplay baru harus memulai pemain di initial spawn Level 1, bukan pada checkpoint terakhir dari sesi sebelumnya.

### 4.3 Checkpoint, Relic, dan Long Jump

- **FR-011 (P1):** Saat pemain menyentuh checkpoint 1, sistem harus mengaktifkan titik respawn checkpoint 1 dan memberi Kunci Kristal.
- **FR-012 (P1):** Saat pemain menyentuh checkpoint 2, sistem harus mengaktifkan titik respawn checkpoint 2 dan memberi Relik Es.
- **FR-013 (P1):** Pengambilan checkpoint harus memicu popup skill dan animasi pickup yang menonaktifkan trigger, menaikkan/membesarkan sprite, memudarkannya, dan menghapus objek setelah animasi.
- **FR-014 (P1):** Pengambilan checkpoint baru harus membuka long jump tingkat sesuai checkpoint dan mengisi tiga charge.
- **FR-015 (P1):** Long jump hanya dapat dilakukan ketika pemain menahan Shift dan menekan tombol lompat, skill sudah terbuka, pemain grounded, dan charge tersedia.
- **FR-016 (P1):** Satu long jump harus mengurangi charge tepat satu. Lompatan normal tidak boleh mengurangi charge.
- **FR-017 (P1):** Checkpoint 2 harus memperbarui tingkat long jump dan mengisi ulang charge menjadi tiga.
- **FR-018 (P2):** HUD harus menampilkan tingkat skill dan jumlah charge yang tersisa.

### 4.4 Hazard, Respawn, dan Finish

- **FR-019 (P1):** Menyentuh trigger duri atau fall-reset harus menghentikan gerak sementara, menampilkan popup, lalu memindahkan pemain ke respawnPosition terakhir.
- **FR-020 (P1):** Respawn harus menjaga progress relic dan tingkat/charge skill dalam sesi berjalan.
- **FR-021 (P1):** EXIT tidak boleh menyelesaikan level jika Kunci Kristal atau Relik Es belum diperoleh; sistem harus menunjukkan item yang masih kurang.
- **FR-022 (P1):** Setelah kedua relic diperoleh dan EXIT disentuh, sistem harus menghentikan pemain dan menampilkan layar kemenangan.
- **FR-023 (P2):** Layar kemenangan harus menampilkan ucapan selamat, kedua relic, skill yang diperoleh, serta aksi replay dan kembali ke menu.
- **FR-024 (P1):** Pemain harus dapat mengulang level dengan R/tombol MAIN LAGI dan kembali ke menu dengan M/tombol MENU UTAMA.

## 5. Kebutuhan Nonfungsional

- **NFR-001 - Usability (P1):** Label tombol dan kontrol harus mudah dipahami, tidak terpotong, dan memiliki kontras yang cukup pada viewport target.
- **NFR-002 - Responsiveness (P2):** Menu harus menyesuaikan ukuran dasar layout terhadap lebar Game view serta mendukung fullscreen dan dua pilihan resolusi yang tersedia.
- **NFR-003 - Performance (P2):** Main Menu menargetkan 60 FPS; efek koleksi dan konfeti harus dibatasi agar tidak menyebabkan penurunan performa yang mengganggu pada perangkat pengembangan.
- **NFR-004 - Reliability (P1):** Pengambilan checkpoint berulang tidak boleh menggandakan relic, charge, atau notifikasi skill yang sama.
- **NFR-005 - Maintainability (P2):** Menu, kontrol pemain, progress, pickup, hazard, dan finish harus berada pada komponen/script terpisah.
- **NFR-006 - Visual quality (P2):** Artwork Main Menu harus tersedia sebagai PNG 3840x2160 dan diimpor tanpa kompresi untuk menjaga detail pixel-art.
- **NFR-007 - Input compatibility (P1):** Input runtime harus menggunakan Unity Input System sesuai konfigurasi Player Settings.

## 6. Model Data dan Status

| Data               | Tipe/aturan                                                                       | Siklus hidup                             |
| ------------------ | --------------------------------------------------------------------------------- | ---------------------------------------- |
| `initialSpawn`     | Posisi world awal Level 1; baseline scene x=-6.5                                  | Tersimpan di SampleScene                 |
| `respawnPosition`  | Posisi spawn atau checkpoint tertinggi yang aktif                                 | Sesi gameplay                            |
| `activeCheckpoint` | 0, 1, atau 2                                                                      | Reset saat Level 1 dimulai               |
| `hasCrystalKey`    | Boolean collectible pertama                                                       | Sesi gameplay                            |
| `hasIceRelic`      | Boolean collectible kedua                                                         | Sesi gameplay                            |
| `jumpUpgradeLevel` | 0 sebelum checkpoint, 1 setelah checkpoint 1, 2 setelah checkpoint 2              | Sesi gameplay                            |
| `longJumpCharges`  | 0 sebelum skill terbuka; 3 setelah checkpoint baru; berkurang satu tiap long jump | Sesi gameplay                            |
| `masterVolume`     | Nilai 0..1                                                                        | Disimpan di PlayerPrefs saat SAVE & BACK |

## 7. Alur Use Case

### UC-01 - Memulai Level

1. Pemain membuka aplikasi.
2. Sistem menampilkan Main Menu.
3. Pemain memilih PLAY.
4. Sistem memuat SampleScene, mengatur progress baru, dan menempatkan pemain pada initial spawn.
5. Sistem menampilkan instruksi misi.

### UC-02 - Mengambil Checkpoint

1. Pemain menyentuh checkpoint.
2. Sistem memperbarui respawnPosition dan collectible terkait.
3. Sistem membuka tingkat long jump yang sesuai dan mengisi tiga charge.
4. Sistem menampilkan popup skill, efek burst, dan animasi pickup sampai objek hilang.

### UC-03 - Menggunakan Long Jump

1. Pemain memastikan skill terbuka dan charge tersedia.
2. Pemain mengarahkan karakter ke rintangan.
3. Pemain menahan Shift dan menekan Space/W/panah atas.
4. Sistem mengurangi charge satu dan menerapkan dorongan long jump.
5. HUD menampilkan jumlah charge terbaru.

### UC-04 - Gagal dan Respawn

1. Pemain menyentuh duri atau jatuh ke fall-reset.
2. Sistem menampilkan notifikasi kematian.
3. Setelah jeda respawn, sistem mengembalikan pemain ke respawnPosition terbaru.
4. Progress relic dan skill sesi tetap tersedia.

### UC-05 - Menyelesaikan Level

1. Pemain mengumpulkan Kunci Kristal dan Relik Es.
2. Pemain menyentuh EXIT.
3. Sistem menghentikan karakter dan menampilkan layar kemenangan.
4. Pemain memilih MAIN LAGI atau MENU UTAMA.

## 8. Kriteria Penerimaan dan Pengujian

| ID     | Skenario                                                  | Hasil yang diharapkan                                                                     |
| ------ | --------------------------------------------------------- | ----------------------------------------------------------------------------------------- |
| AT-001 | Jalankan build dari Main Menu                             | Main Menu muncul sebelum gameplay.                                                        |
| AT-002 | Pilih PLAY                                                | SampleScene dimuat dan pemain berada di initial spawn.                                    |
| AT-003 | Tekan Space saat grounded                                 | Lompatan normal terjadi tanpa memakai charge.                                             |
| AT-004 | Ambil checkpoint 1                                        | Kunci didapat, popup/animasi tampil, Long Jump I terbuka, charge menjadi 3.               |
| AT-005 | Gunakan Shift+Space                                       | Long jump bergerak lebih jauh dan charge berkurang satu.                                  |
| AT-006 | Gunakan long jump tanpa charge atau sebelum skill terbuka | Long jump khusus tidak aktif; input tetap mengikuti aturan normal jump.                   |
| AT-007 | Ambil checkpoint 2                                        | Relik didapat, Long Jump II terbuka, charge terisi kembali menjadi 3.                     |
| AT-008 | Sentuh duri atau jatuh                                    | Pemain respawn pada checkpoint terakhir dan relic/skill sesi tetap ada.                   |
| AT-009 | Sentuh EXIT tanpa satu atau kedua relic                   | Level tidak selesai; popup menyebut relic yang belum didapat.                             |
| AT-010 | Kumpulkan dua relic lalu sentuh EXIT                      | Layar kemenangan, kedua kartu relic, skill, dan tombol aksi terlihat.                     |
| AT-011 | Pilih MAIN LAGI atau tekan R                              | Level dimuat ulang dari initial spawn dan progress sesi direset.                          |
| AT-012 | Pilih MENU UTAMA atau tekan M                             | Main Menu dimuat.                                                                         |
| AT-013 | Ubah volume dan pilih SAVE & BACK                         | Volume diterapkan dan tersimpan untuk sesi berikutnya.                                    |
| AT-014 | Ubah fullscreen dan resolusi di Options                   | Display mengikuti pilihan fullscreen/resolusi yang dipilih.                               |
| AT-015 | Buka LEVEL SELECT                                         | Level 01 tampil sebagai level aktif dan tombol PLAY LEVEL 01 memuat SampleScene.          |
| AT-016 | Gunakan tiga long jump berturut-turut setelah checkpoint  | Charge turun 3/3, 2/3, 1/3, lalu 0/3; long jump berikutnya tidak memakai aksi khusus.     |
| AT-017 | Buka Options pada viewport target                         | Kontrol dan tombol simpan terlihat tanpa overlap/cropping.                                |
| AT-018 | Pilih QUIT pada build                                     | Aplikasi berhenti; pada Unity Editor, Play Mode berhenti.                                 |
| AT-019 | Bergerak dan melompat di Level 1                          | Main Camera mengikuti posisi pemain dengan smoothing dan mempertahankan kedalaman kamera. |

Status pengujian pada dokumen ini adalah **prosedur penerimaan**; setiap skenario tetap perlu dijalankan pada target build final.

### 8.1 Matriks Traceability

| Kebutuhan              | Acceptance test                                        |
| ---------------------- | ------------------------------------------------------ |
| FR-001 sampai FR-006   | AT-001, AT-002, AT-013, AT-014, AT-015, AT-017, AT-018 |
| FR-007, FR-008, FR-010 | AT-002, AT-003                                         |
| FR-009                 | AT-019                                                 |
| FR-011 sampai FR-014   | AT-004, AT-007                                         |
| FR-015 sampai FR-018   | AT-005, AT-006, AT-007, AT-016                         |
| FR-019 sampai FR-020   | AT-008                                                 |
| FR-021 sampai FR-024   | AT-009, AT-010, AT-011, AT-012                         |

## 9. Pemetaan Kebutuhan ke Implementasi

| Area                             | Implementasi utama                                                   |
| -------------------------------- | -------------------------------------------------------------------- |
| Menu, Options, scene start       | `Assets/Scripts/Level1MainMenu.cs`                                   |
| Gerak, lompatan, skill/charge    | `Assets/Scripts/NinjaGirlController.cs`                              |
| Objective, HUD, respawn, victory | `Assets/Scripts/GameProgressManager.cs`                              |
| Pickup dan animasi koleksi       | `Assets/Scripts/CheckpointTrigger.cs`                                |
| Hazard dan finish                | `Assets/Scripts/HazardTrigger.cs`, `Assets/Scripts/FinishTrigger.cs` |
| Scene menu                       | `Assets/Main Menu.unity`                                             |
| Scene level                      | `Assets/Scenes/SampleScene.unity`                                    |

## 10. Batasan dan Perluasan Mendatang

- Saat ini hanya satu level yang dapat dipilih.
- Multiplayer/Relay, lobby, penyimpanan progress lintas sesi, controller, mobile touch, audio musik/SFX terpisah, serta level tambahan belum menjadi bagian baseline aktif.
- Resolusi final dan target build distribusi harus ditetapkan saat proses Build Profiles.
- Perlu dilakukan smoke test menyeluruh pada build final, khususnya perpindahan scene Main Menu -> Level 1, pengambilan dua relic, pemakaian charge long jump, kematian/respawn, dan kemenangan.
