Dự án CNTT - Project Pacman Multiplayer 
Thành viên:
- 521h0512 - Đặng Xuân Thịnh
- 520H0451 - Nguyễn Chí Cường

Cách chạy project:

* Nếu chạy bằng Unity Editor
- Bật project bằng Unity
- Vào scene MainMenu trong Unity
- Ấn nút Play vào Game mode để chạy game

* Nếu chạy bằng bản Build
- Vào folder Builds/PacmanMultiplayer
- Chạy file Pac Adventure.exe

Note: Do đây là game multiplayer nên sẽ cần phải chạy nhiều build cùng lúc để chơi multiplayer, có thể chơi single player được bằng cách tạo lobby rồi vào chơi như bình thường, có thể bật một lúc nhiều build để chạy
 hoặc là đưa bản build cho một người khác để 2 người cùng chơi chung với nhau (khuyến khích chơi với người khác)
 để bật một lúc nhiều build, có thể chạy một build bằng Unity Editor, build còn lại thì chạy file Pac Adventure.exe, hoặc
 là chạy file Pac Adventure.exe nhiều lần để bật một lúc nhiều cửa sổ game

Lúc vào được game sẽ vào scene MainMenu, trong scene có 3 tab, Achievements, Multiplayer, Options
- Tab Achievement: sẽ hiện cửa sổ hiện các achievement trong game, khi chơi và thỏa điều kiện thì achievement sẽ được đánh dấu là hoàn thành
- Tab Options: chứa 2 slider để tăng/giảm âm lượng game
- Tab Multiplayer: ấn vào sẽ hiện cửa sổ Login, sau khi tạo tài khoản và login thì sẽ được chuyển tới scene Lobby, trong scene Lobby player có thể tự tạo room hoặc join room của
người khác

* Khi tạo room có thể điều chỉnh tên room, chọn map, chỉnh room thành public/private, điều chỉnh chế độ chơi giữa Classic và Survival, chỉnh độ khó AI, 
độ khó AI càng cao thì 4 con ma sẽ càng hung dữ và chạy lẹ hơn (nếu không có kinh nghiệm chơi Pacman thì nên chọn AI là Easy để dễ test game hơn)

Sau khi join room hoặc tạo room sẽ được chuyển tới scene CharacterSelect, trong đây có thể chỉnh model nhân vật, player khác khi join room cũng sẽ hiện trong scene
Khi tất cả các player đều đã ấn nút Ready thì chủ room/host có thể ấn nút Start Game để bắt đầu chơi game

* Khi ở trong scene CharacterSelect hoặc trong scene chơi game, nếu chủ room/host tắt game/disconnect thì tất cả client khác đều sẽ bị disconnect

* Cách chơi:
- Di chuyển bằng mũi tên hoặc WASD
- Mục tiêu là ăn thật nhiều pellet và các power-up và tránh bị 4 con ma bắt
- Trong chế độ Classic, chỉ cần ăn hết pellet trên map là thắng
- Trong chế độ Survival, sẽ giống như Classic nhưng sẽ phải sống sót trong 4 phút thì mới thắng, lâu lâu ở giữa map sẽ spawn quả dâu, khi player chạm vào thì sẽ respawn hết tất cả pellet đã ăn trên map
- Mỗi player có 3 mạng, nếu bị ma bắt 3 lần thì Game Over
- Khi đang chơi nếu player thỏa điều kiện các achievement thì sẽ có một UI pop up báo hiệu achievement complete
