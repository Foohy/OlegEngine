﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

using OpenTK;
using OpenTK.Graphics.OpenGL;



namespace OlegEngine
{
    public class EngineResources
    {
        #region static resources
        const string Quad = 
@"v 0.999999 1.000001 -0.000000
v -0.000001 0.999999 0.000001
v 1.000001 0.000001 -0.000001
v 0.000001 -0.000001 0.000000
vt 0.000000 0.000000
vt 1.000000 0.000000
vt 1.000000 1.000000
vt 0.000000 1.000000
vn -0.000001 0.000001 -1.000000
f 2/1/1 1/2/1 3/3/1
f 2/1/1 3/3/1 4/4/1";

        const string Box = 
@"v -1.000000 -1.000000 1.000000
v -1.000000 -1.000000 -1.000000
v 1.000000 -1.000000 -1.000000
v 1.000000 -1.000000 1.000000
v -1.000000 1.000000 1.000000
v -1.000000 1.000000 -1.000000
v 1.000000 1.000000 -1.000000
v 1.000000 1.000000 1.000000
vn -1.000000 -0.000000 0.000000
vn 0.000000 0.000000 -1.000000
vn 1.000000 -0.000000 0.000000
vn 0.000000 0.000000 1.000000
vn -0.000000 -1.000000 0.000000
vn -0.000000 1.000000 0.000000
s off
f 2//1 1//1 5//1
f 2//1 5//1 6//1
f 6//2 7//2 3//2
f 6//2 3//2 2//2
f 7//3 8//3 4//3
f 7//3 4//3 3//3
f 1//4 4//4 8//4
f 1//4 8//4 5//4
f 1//5 2//5 3//5
f 1//5 3//5 4//5
f 8//6 7//6 6//6
f 8//6 6//6 5//6";

        const string Skybox =
@"v -1.000000 -1.000000 1.000000
v -1.000000 -1.000000 -1.000000
v 1.000000 -1.000000 -1.000000
v 1.000000 -1.000000 1.000000
v -1.000000 1.000000 1.000000
v -1.000000 1.000000 -1.000000
v 1.000000 1.000000 -1.000000
v 1.000000 1.000000 1.000000
f 2 1 5
f 2 5 6
f 6 7 3
f 6 3 2
f 7 8 4
f 7 4 3
f 1 4 8
f 1 8 5
f 8 7 6
f 8 6 5
f 3 4 1
f 1 2 3";

        const string Ball = 
@"v 0.000000 -1.000000 0.000000
v 0.723607 -0.447220 0.525725
v -0.276388 -0.447220 0.850649
v -0.894426 -0.447216 0.000000
v -0.276388 -0.447220 -0.850649
v 0.723607 -0.447220 -0.525725
v 0.276388 0.447220 0.850649
v -0.723607 0.447220 0.525725
v -0.723607 0.447220 -0.525725
v 0.276388 0.447220 -0.850649
v 0.894426 0.447216 0.000000
v 0.000000 1.000000 0.000000
v 0.425323 -0.850654 0.309011
v 0.262869 -0.525738 0.809012
v -0.162456 -0.850654 0.499995
v 0.425323 -0.850654 -0.309011
v 0.850648 -0.525736 0.000000
v -0.688189 -0.525736 0.499997
v -0.525730 -0.850652 0.000000
v -0.688189 -0.525736 -0.499997
v -0.162456 -0.850654 -0.499995
v 0.262869 -0.525738 -0.809012
v 0.951058 0.000000 -0.309013
v 0.951058 0.000000 0.309013
v 0.587786 0.000000 0.809017
v 0.000000 0.000000 1.000000
v -0.587786 0.000000 0.809017
v -0.951058 0.000000 0.309013
v -0.951058 0.000000 -0.309013
v -0.587786 0.000000 -0.809017
v 0.000000 0.000000 -1.000000
v 0.587786 0.000000 -0.809017
v 0.688189 0.525736 0.499997
v -0.262869 0.525738 0.809012
v -0.850648 0.525736 0.000000
v -0.262869 0.525738 -0.809012
v 0.688189 0.525736 -0.499997
v 0.525730 0.850652 0.000000
v 0.162456 0.850654 0.499995
v -0.425323 0.850654 0.309011
v -0.425323 0.850654 -0.309011
v 0.162456 0.850654 -0.499995
vt 0.629138 0.900094
vt 0.629138 0.999217
vt 0.536685 0.935839
vt 0.854071 0.453848
vt 0.854071 0.354724
vt 0.946524 0.418103
vt 0.056831 0.577312
vt 0.112875 0.659069
vt 0.000784 0.659069
vt 0.213566 0.977535
vt 0.114442 0.977535
vt 0.177821 0.885082
vt 0.579765 0.703819
vt 0.523720 0.622062
vt 0.635810 0.622062
vt 0.114442 0.784391
vt 0.206896 0.820136
vt 0.114442 0.883516
vt 0.013752 0.743959
vt 0.112875 0.743959
vt 0.049496 0.836414
vt 0.753382 0.361393
vt 0.852505 0.361393
vt 0.789126 0.453848
vt 0.637677 0.226534
vt 0.637677 0.127410
vt 0.730132 0.190787
vt 0.731398 0.815207
vt 0.731398 0.716084
vt 0.823853 0.779461
vt 0.751816 0.361393
vt 0.716069 0.453848
vt 0.652690 0.361393
vt 0.170489 0.440398
vt 0.226534 0.522157
vt 0.114442 0.522157
vt 0.367468 0.650831
vt 0.311423 0.569073
vt 0.423513 0.569073
vt 0.215133 0.789495
vt 0.215133 0.690370
vt 0.307587 0.753749
vt 0.429699 0.569072
vt 0.522154 0.604819
vt 0.429699 0.668197
vt 0.579765 0.455414
vt 0.635812 0.537172
vt 0.523720 0.537172
vt 0.536685 0.898528
vt 0.536685 0.799405
vt 0.629139 0.862782
vt 0.924841 0.106577
vt 0.832388 0.070832
vt 0.924841 0.007453
vt 0.924543 0.808537
vt 0.825419 0.808537
vt 0.888797 0.716084
vt 0.013752 0.837980
vt 0.112875 0.837980
vt 0.049498 0.930434
vt 0.666454 0.816773
vt 0.729832 0.909227
vt 0.630709 0.909227
vt 0.341756 0.341757
vt 0.438829 0.397802
vt 0.341756 0.453848
vt 0.635811 0.538738
vt 0.579765 0.620496
vt 0.523720 0.538738
vt 0.838756 0.678772
vt 0.931209 0.615394
vt 0.931209 0.714518
vt 0.425381 0.226534
vt 0.425381 0.114443
vt 0.522454 0.170487
vt 0.000786 0.742393
vt 0.056831 0.660636
vt 0.112875 0.742393
vt 0.924542 0.554537
vt 0.832089 0.491160
vt 0.924542 0.455414
vt 0.129460 0.326741
vt 0.226534 0.382786
vt 0.129460 0.438832
vt 0.552488 0.228100
vt 0.496443 0.309858
vt 0.440397 0.228100
vt 0.112875 0.575746
vt 0.000783 0.575746
vt 0.056830 0.493988
vt 0.423815 0.114443
vt 0.423815 0.226534
vt 0.326741 0.170490
vt 0.868134 0.219863
vt 0.832388 0.127410
vt 0.931511 0.127410
vt 0.731398 0.816773
vt 0.823852 0.880150
vt 0.731398 0.915896
vt 0.112875 0.492422
vt 0.000785 0.492422
vt 0.056830 0.395348
vt 0.360136 0.922196
vt 0.324390 0.829742
vt 0.423513 0.829742
vt 0.539034 0.372090
vt 0.651124 0.372090
vt 0.595079 0.453848
vt 0.228100 0.325175
vt 0.284145 0.228100
vt 0.340190 0.325175
vt 0.435994 0.799405
vt 0.535117 0.799405
vt 0.499371 0.891860
vt 0.471741 0.705385
vt 0.535119 0.797839
vt 0.435994 0.797839
vt 0.213083 0.056831
vt 0.310157 0.000783
vt 0.310157 0.112875
vt 0.243415 0.440398
vt 0.325173 0.496443
vt 0.243415 0.552491
vt 0.760166 0.263847
vt 0.667712 0.327225
vt 0.667712 0.228100
vt 0.114442 0.114442
vt 0.211517 0.170488
vt 0.114442 0.226534
vt 0.730133 0.000784
vt 0.730133 0.099908
vt 0.637677 0.064162
vt 0.177820 0.782825
vt 0.114442 0.690370
vt 0.213567 0.690370
vt 0.000783 0.000783
vt 0.112876 0.000783
vt 0.056829 0.097858
vt 0.341756 0.455414
vt 0.423513 0.511460
vt 0.341756 0.567507
vt 0.635810 0.705385
vt 0.572432 0.797839
vt 0.536685 0.705385
vt 0.112875 0.198065
vt 0.056830 0.295141
vt 0.000784 0.198065
vt 0.830822 0.099908
vt 0.731699 0.099908
vt 0.767443 0.007453
vt 0.673124 0.455414
vt 0.736502 0.547869
vt 0.637378 0.547869
vt 0.114442 0.056829
vt 0.211517 0.000783
vt 0.211517 0.112876
vt 0.801446 0.622063
vt 0.837190 0.714517
vt 0.738067 0.714517
vt 0.522154 0.455414
vt 0.522154 0.567506
vt 0.440396 0.511462
vt 0.114442 0.325175
vt 0.170486 0.228100
vt 0.226534 0.325175
vt 0.387769 0.735721
vt 0.423513 0.828176
vt 0.324390 0.828176
vt 0.860857 0.234772
vt 0.797477 0.327225
vt 0.761732 0.234772
vt 0.112875 0.393782
vt 0.000785 0.393782
vt 0.056830 0.296707
vt 0.215133 0.826807
vt 0.307588 0.791062
vt 0.307588 0.890184
vt 0.795078 0.219865
vt 0.731698 0.127410
vt 0.830822 0.127410
vt 0.269128 0.226534
vt 0.213083 0.129459
vt 0.325175 0.129459
vt 0.637376 0.751829
vt 0.729832 0.716084
vt 0.729832 0.815207
vt 0.830523 0.491158
vt 0.738068 0.554537
vt 0.738068 0.554537
vt 0.738068 0.455414
vt 0.000783 0.099424
vt 0.112875 0.099424
vt 0.056829 0.196499
vt 0.700754 0.622062
vt 0.736501 0.714517
vt 0.637376 0.714518
vt 0.554054 0.228100
vt 0.666146 0.228100
vt 0.610099 0.309857
vt 0.341756 0.284146
vt 0.438830 0.228100
vt 0.438831 0.340191
vt 0.524020 0.226534
vt 0.580065 0.144776
vt 0.636111 0.226534
vt 0.114442 0.607047
vt 0.226533 0.607047
vt 0.170488 0.688804
vt 0.410364 0.097858
vt 0.466409 0.000784
vt 0.522454 0.097858
vt 0.825418 0.816773
vt 0.924541 0.816773
vt 0.888795 0.909227
vt 0.423513 0.734155
vt 0.311424 0.734155
vt 0.367469 0.652397
vt 0.537468 0.397801
vt 0.440396 0.453848
vt 0.440396 0.341757
vt 0.961545 0.327226
vt 0.862423 0.327226
vt 0.898168 0.234772
vt 0.528447 0.992549
vt 0.435994 0.929171
vt 0.528447 0.893426
vt 0.325173 0.382786
vt 0.228100 0.438832
vt 0.228100 0.326741
vt 0.636111 0.000784
vt 0.580066 0.082542
vt 0.524020 0.000784
vt 0.314256 0.891750
vt 0.250877 0.984203
vt 0.215133 0.891750
vt 0.311724 0.000784
vt 0.408798 0.056829
vt 0.311724 0.112875
vt 0.226534 0.523723
vt 0.170488 0.605481
vt 0.114442 0.523723
vn 0.102381 -0.943523 0.315090
vn 0.700224 -0.661699 0.268032
vn -0.268034 -0.943523 0.194737
vn -0.268034 -0.943523 -0.194737
vn 0.102381 -0.943523 -0.315090
vn 0.904989 -0.330384 0.268031
vn 0.024747 -0.330386 0.943521
vn -0.889697 -0.330385 0.315095
vn -0.574602 -0.330388 -0.748784
vn 0.534576 -0.330386 -0.777865
vn 0.802609 -0.125627 0.583126
vn -0.306569 -0.125629 0.943522
vn -0.992077 -0.125628 0.000000
vn -0.306569 -0.125629 -0.943522
vn 0.802609 -0.125627 -0.583126
vn 0.408946 0.661698 0.628425
vn -0.471300 0.661699 0.583122
vn -0.700224 0.661699 -0.268032
vn 0.038530 0.661699 -0.748779
vn 0.724042 0.661695 -0.194736
vn -0.038530 -0.661699 0.748779
vn 0.187594 -0.794658 0.577345
vn 0.471300 -0.661699 0.583122
vn 0.700224 -0.661699 -0.268032
vn 0.607060 -0.794656 0.000000
vn 0.331305 -0.943524 0.000000
vn -0.724042 -0.661695 0.194736
vn -0.491119 -0.794657 0.356821
vn -0.408946 -0.661698 0.628425
vn -0.408946 -0.661698 -0.628425
vn -0.491119 -0.794657 -0.356821
vn -0.724042 -0.661695 -0.194736
vn 0.471300 -0.661699 -0.583122
vn 0.187594 -0.794658 -0.577345
vn -0.038530 -0.661699 -0.748779
vn 0.992077 0.125628 -0.000000
vn 0.982246 -0.187599 0.000000
vn 0.904989 -0.330384 -0.268031
vn 0.306569 0.125629 0.943522
vn 0.303531 -0.187597 0.934171
vn 0.534576 -0.330386 0.777865
vn -0.802609 0.125627 0.583126
vn -0.794656 -0.187595 0.577348
vn -0.574602 -0.330388 0.748784
vn -0.802609 0.125627 -0.583126
vn -0.794656 -0.187595 -0.577348
vn -0.889697 -0.330385 -0.315095
vn 0.306569 0.125629 -0.943522
vn 0.303531 -0.187597 -0.934171
vn 0.024747 -0.330386 -0.943521
vn 0.574602 0.330388 0.748784
vn 0.794656 0.187595 0.577348
vn 0.889697 0.330385 0.315095
vn -0.534576 0.330386 0.777865
vn -0.303531 0.187597 0.934171
vn -0.024747 0.330386 0.943521
vn -0.904989 0.330384 -0.268031
vn -0.982246 0.187599 0.000000
vn -0.904989 0.330384 0.268031
vn -0.024747 0.330386 -0.943521
vn -0.303531 0.187597 -0.934171
vn -0.534576 0.330386 -0.777865
vn 0.889697 0.330385 -0.315095
vn 0.794656 0.187595 -0.577348
vn 0.574602 0.330388 -0.748784
vn 0.268034 0.943523 0.194737
vn 0.491119 0.794657 0.356821
vn 0.724042 0.661695 0.194736
vn -0.102381 0.943523 0.315090
vn -0.187594 0.794658 0.577345
vn 0.038530 0.661699 0.748779
vn -0.331305 0.943524 0.000000
vn -0.607060 0.794656 0.000000
vn -0.700224 0.661699 0.268032
vn -0.102381 0.943523 -0.315090
vn -0.187594 0.794658 -0.577345
vn -0.471300 0.661699 -0.583122
vn 0.268034 0.943523 -0.194737
vn 0.491119 0.794657 -0.356821
vn 0.408946 0.661698 -0.628425
f 1/1/1 13/2/1 15/3/1
f 2/4/2 13/5/2 17/6/2
f 1/7/3 15/8/3 19/9/3
f 1/10/4 19/11/4 21/12/4
f 1/13/5 21/14/5 16/15/5
f 2/16/6 17/17/6 24/18/6
f 3/19/7 14/20/7 26/21/7
f 4/22/8 18/23/8 28/24/8
f 5/25/9 20/26/9 30/27/9
f 6/28/10 22/29/10 32/30/10
f 2/31/11 24/32/11 25/33/11
f 3/34/12 26/35/12 27/36/12
f 4/37/13 28/38/13 29/39/13
f 5/40/14 30/41/14 31/42/14
f 6/43/15 32/44/15 23/45/15
f 7/46/16 33/47/16 39/48/16
f 8/49/17 34/50/17 40/51/17
f 9/52/18 35/53/18 41/54/18
f 10/55/19 36/56/19 42/57/19
f 11/58/20 37/59/20 38/60/20
f 15/61/21 14/62/21 3/63/21
f 15/64/22 13/65/22 14/66/22
f 13/67/23 2/68/23 14/69/23
f 17/70/24 16/71/24 6/72/24
f 17/73/25 13/74/25 16/75/25
f 13/76/26 1/77/26 16/78/26
f 19/79/27 18/80/27 4/81/27
f 19/82/28 15/83/28 18/84/28
f 15/85/29 3/86/29 18/87/29
f 21/88/30 20/89/30 5/90/30
f 21/91/31 19/92/31 20/93/31
f 19/94/32 4/95/32 20/96/32
f 16/97/33 22/98/33 6/99/33
f 16/100/34 21/101/34 22/102/34
f 21/103/35 5/104/35 22/105/35
f 24/106/36 23/107/36 11/108/36
f 24/109/37 17/110/37 23/111/37
f 17/112/38 6/113/38 23/114/38
f 26/115/39 25/116/39 7/117/39
f 26/118/40 14/119/40 25/120/40
f 14/121/41 2/122/41 25/123/41
f 28/124/42 27/125/42 8/126/42
f 28/127/43 18/128/43 27/129/43
f 18/130/44 3/131/44 27/132/44
f 30/133/45 29/134/45 9/135/45
f 30/136/46 20/137/46 29/138/46
f 20/139/47 4/140/47 29/141/47
f 32/142/48 31/143/48 10/144/48
f 32/145/49 22/146/49 31/147/49
f 22/148/50 5/149/50 31/150/50
f 25/151/51 33/152/51 7/153/51
f 25/154/52 24/155/52 33/156/52
f 24/157/53 11/158/53 33/159/53
f 27/160/54 34/161/54 8/162/54
f 27/163/55 26/164/55 34/165/55
f 26/166/56 7/167/56 34/168/56
f 29/169/57 35/170/57 9/171/57
f 29/172/58 28/173/58 35/174/58
f 28/175/59 8/176/59 35/177/59
f 31/178/60 36/179/60 10/180/60
f 31/181/61 30/182/61 36/183/61
f 30/184/62 9/185/62 36/186/62
f 23/187/63 37/188/63 11/189/63
f 23/190/64 32/191/64 37/192/64
f 32/193/65 10/194/65 37/195/65
f 39/196/66 38/197/66 12/198/66
f 39/199/67 33/200/67 38/201/67
f 33/202/68 11/203/68 38/204/68
f 40/205/69 39/206/69 12/207/69
f 40/208/70 34/209/70 39/210/70
f 34/211/71 7/212/71 39/213/71
f 41/214/72 40/215/72 12/216/72
f 41/217/73 35/218/73 40/219/73
f 35/220/74 8/221/74 40/222/74
f 42/223/75 41/224/75 12/225/75
f 42/226/76 36/227/76 41/228/76
f 36/229/77 9/230/77 41/231/77
f 38/232/78 42/233/78 12/234/78
f 38/235/79 37/236/79 42/237/79
f 37/238/80 10/239/80 42/240/80";

        #endregion

        /// <summary>
        /// Internal class to create engine-related sources
        /// Do not call unless you know exactly what you're doing
        /// </summary>
        public static void CreateResources()
        {
            #region Important materials for the normal workings of the engine

            //Create some useful materials
            Utilities.ErrorTex = GenerateErrorTex();
            Utilities.White = GenerateColor(Color.White);
            Utilities.Black = GenerateColor(Color.Black);
            Utilities.NormalTex = GenerateNormalTex();
            Utilities.SpecTex = Utilities.White;
            Utilities.AlphaTex = Utilities.White;
            Utilities.DefaultSkyboxTex = GenerateSolidSkybox(Color.Black);
            Utilities.ErrorMat = new Material(Utilities.ErrorTex, "default");
            Utilities.NormalMat = new Material(Utilities.NormalTex, "default");

            #endregion


            #region Basic/Debug models

            Vertex[] verts;
            int[] elements;
            Mesh.BoundingBox boundingbox;

            Resource.InsertMesh("engine/quad.obj", CreateNewQuadMesh());

            MeshGenerator.LoadOBJFromString(Box, out verts, out elements, out boundingbox);
            Mesh mBox = new Mesh(verts, elements);
            mBox.BBox = boundingbox;
            Resource.InsertMesh("engine/box.obj", mBox);

            MeshGenerator.LoadOBJFromString(Ball, out verts, out elements, out boundingbox);
            Mesh mBall = new Mesh(verts, elements);
            mBall.BBox = boundingbox;
            Resource.InsertMesh("engine/ball.obj", mBall);

            MeshGenerator.LoadOBJFromString(Skybox, out verts, out elements, out boundingbox);
            Mesh mSkybox = new Mesh(verts, elements);
            mSkybox.BBox = boundingbox;
            Resource.InsertMesh("engine/skybox.obj", mSkybox);

            #endregion


            #region Engine-based and 'utility' materials

            MaterialProperties properties = new MaterialProperties();
            properties.ShaderProgram = Resource.GetProgram("default_lighting");
            properties.BaseTexture = Utilities.White;
            Material white = new Material(properties);
            Resource.InsertMaterial("engine/white", white);

            properties = new MaterialProperties();
            properties.ShaderProgram = Resource.GetProgram("default");
            properties.BaseTexture = Utilities.White;
            Material white_simple = new Material(properties);
            Resource.InsertMaterial("engine/white_simple", white_simple);

            properties = new MaterialProperties();
            properties.ShaderProgram = Resource.GetProgram("default_lighting");
            properties.BaseTexture = Utilities.White;
            properties.SpecularPower = 32.0f;
            properties.SpecularIntensity = 4.0f;
            Material white_shiny = new Material(properties);
            Resource.InsertMaterial("engine/white_shiny", white_shiny);

            properties = new MaterialProperties();
            properties.ShaderProgram = Resource.GetProgram("depthtest");
            Material depthtest = new Material(properties);
            Resource.InsertMaterial("engine/depth", depthtest);

            #endregion
        }

        /// <summary>
        /// Create a new quad mesh outside of the resource manager
        /// </summary>
        /// <returns>Mesh quad</returns>
        public static Mesh CreateNewQuadMesh()
        {
            Vertex[] verts;
            int[] elements;
            Mesh.BoundingBox boundingbox;

            MeshGenerator.LoadOBJFromString(Quad, out verts, out elements, out boundingbox);
            return new Mesh(verts, elements);
        }

        /// <summary>
        /// Generate the error texture to be displayed when a texture could not be found
        /// </summary>
        /// <returns>Erro texture</returns>
        private static int GenerateErrorTex()
        {
            Bitmap tex = new Bitmap(32, 32);
            for (int x = 0; x < tex.Height; x++)
            {
                for (int y = 0; y < tex.Width; y++)
                {
                    if ((x + (y % 2)) % 2 == 0)
                    {
                        tex.SetPixel(x, y, Color.White);
                    }
                    else
                    {
                        tex.SetPixel(x, y, Color.Red);
                    }
                }
            }

            //Create the opengl texture
            GL.ActiveTexture(TextureUnit.Texture6);//Something farish away
            int id = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, id);

            System.Drawing.Imaging.BitmapData bmp_data = tex.LockBits(new Rectangle(0, 0, tex.Width, tex.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bmp_data.Width, bmp_data.Height, 0,
                OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, bmp_data.Scan0);

            tex.UnlockBits(bmp_data);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.ActiveTexture(TextureUnit.Texture0);
            return id;
        }

        /// <summary>
        /// Generate the normal texture that defines that all normals are facing up. Used when a material has no normal map
        /// </summary>
        /// <returns>Default normal map</returns>
        private static int GenerateNormalTex()
        {
            Bitmap tex = new Bitmap(1, 1);

            tex.SetPixel(0, 0, Color.FromArgb(133, 119, 253));

            //Create the opengl texture
            GL.ActiveTexture(TextureUnit.Texture6);//Something farish away
            int id = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, id);

            System.Drawing.Imaging.BitmapData bmp_data = tex.LockBits(new Rectangle(0, 0, tex.Width, tex.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bmp_data.Width, bmp_data.Height, 0,
                OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, bmp_data.Scan0);

            tex.UnlockBits(bmp_data);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.ActiveTexture(TextureUnit.Texture0);
            return id;
        }

        /// <summary>
        /// A completely white texture.
        /// </summary>
        /// <returns>White 1x1 texture</returns>
        private static int GenerateColor( Color col)
        {
            Bitmap tex = new Bitmap(1, 1);

            tex.SetPixel(0, 0, col);

            //Create the opengl texture
            GL.ActiveTexture(TextureUnit.Texture6);//Something farish away
            int id = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, id);

            System.Drawing.Imaging.BitmapData bmp_data = tex.LockBits(new Rectangle(0, 0, tex.Width, tex.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bmp_data.Width, bmp_data.Height, 0,
                OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, bmp_data.Scan0);

            tex.UnlockBits(bmp_data);
            tex.Dispose();

            // We haven't uploaded mipmaps, so disable mipmapping (otherwise the texture will not appear).
            // On newer video cards, we can use GL.GenerateMipmaps() or GL.Ext.GenerateMipmaps() to create
            // mipmaps automatically. In that case, use TextureMinFilter.LinearMipmapLinear to enable them.
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.ActiveTexture(TextureUnit.Texture0);
            return id;
        }

        /// <summary>
        /// Generate a default skybox texture with the same color for all sides
        /// </summary>
        /// <param name="col"></param>
        /// <returns></returns>
        private static int GenerateSolidSkybox(Color col)
        {
            int tex;
            //Generate the opengl texture
            GL.ActiveTexture(TextureUnit.Texture6);
            tex = GL.GenTexture();
            GL.BindTexture(TextureTarget.TextureCubeMap, tex);

            for (int i = 0; i < Utilities._cubeMapTargets.Length; i++)
            {
                Bitmap bmp = new Bitmap(1, 1);
                bmp.SetPixel(0, 0, col);

                if (bmp == null)
                {
                    //alright fine whatever jerk
                    return Utilities.ErrorTex;
                }

                //Load the data into it
                System.Drawing.Imaging.BitmapData bmp_data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                GL.TexImage2D(Utilities._cubeMapTargets[i], 0, PixelInternalFormat.Rgba, bmp_data.Width, bmp_data.Height, 0,
                    PixelFormat.Bgra, PixelType.UnsignedByte, bmp_data.Scan0);

                bmp.UnlockBits(bmp_data);
                bmp.Dispose();
            }

            //Add some parameters
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapR, (int)TextureWrapMode.ClampToEdge);

            return tex;
        }
    }
}
