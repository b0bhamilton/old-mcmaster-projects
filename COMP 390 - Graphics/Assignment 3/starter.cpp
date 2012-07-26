//Robert Hamilton
//Athabasca University: COMP 390
//TME 3

#include <windows.h>
#include <gl\glut.h>
#include <iostream>
#include <fstream>

using namespace std;

int W;
int H;

const int PixMapChannels = 3;

int PixmapWidth, PixmapHeight;
int AdjustedPixmapWidth;

//char infileName[256] = "lake.bmp";
char infileName[256] = "Impressionists09.bmp";

GLubyte *pixels, *pixcopy;
int texturesize;

void save_ppm();

/*
   The following code assumes a file formatted exactly like
   the given file, and does not consider the possibility of
   comments in the middle of the file, etc.
*/

bool
initialize(void)
{

	BITMAPFILEHEADER bmheader;
    	int i;

	ifstream fp( infileName, ios::binary);

 /*
  * Try opening the file; use "rb" mode to read this *binary* file.
  */


	if ( !fp )
	{
		cout << "cannot open file\n";
	    return false;
	}

	fp.read( reinterpret_cast<char *>(&bmheader), sizeof(BITMAPFILEHEADER));
	if ( fp.gcount() != sizeof(BITMAPFILEHEADER) )
	{

		fp.close();
		return false;
	}

	if ( bmheader.bfType != 'MB')	// Microsoft Only feature
	{
		cout << "Unsuccessful read of bfType\n";
		fp.close();
		return false;
	}

	int infosize = bmheader.bfOffBits - sizeof( BITMAPFILEHEADER ); //_bmpheader.bfOffBits
	int calc = bmheader.bfSize - bmheader.bfOffBits;            // temp value for texture size

	BITMAPINFO *bminfo;

	if (( bminfo = reinterpret_cast<BITMAPINFO *>(new char[ infosize ])) == NULL)
	{
		cout << "Not enough memory for bitmap info\n";
		fp.close();
		return false;
	}
	fp.read( reinterpret_cast<char *>(bminfo), infosize );

	if ( fp.gcount() != infosize )
	{
	    delete [] bminfo;	
	    fp.close();
		return false;
	}

 
	PixmapHeight = bminfo->bmiHeader.biHeight;
	PixmapWidth = bminfo->bmiHeader.biWidth;

	switch( bminfo->bmiHeader.biBitCount ) {
	case 24:
		break;
	default:
		cout << "Cannot read this bitCount\n";
		delete [] bminfo;
		fp.close();
		return false;
	}

	// Handle compression 
	switch( bminfo->bmiHeader.biCompression )
	{
	case BI_RGB:
		break;
	default:	  //printf("BI_JPEG");
		cout << " Cannot handle this compression\n";
		delete [] bminfo;
		fp.close();
		return false;
	}

    if (( texturesize = bminfo->bmiHeader.biSizeImage) == 0)
		texturesize = calc;


 	if ( ( pixels = new GLubyte[texturesize] ) == NULL)
	{
	    delete [] bminfo;
		bminfo = 0;
		fp.close();
		return false;
	}

	cout << "reading " << texturesize << " pixels\n";
	fp.read( reinterpret_cast<char *>(pixels), texturesize );
	if (fp.gcount() != texturesize)
	{
		delete [] bminfo;
	    delete [] pixels;
		bminfo = 0;
		pixels = 0;
		return false;
	 }	
	
    fp.close();

	GLubyte tmp; // for bgr-rgb conversion
	for (i = 0; i < texturesize; i += 3) {
		tmp = pixels[i];
		pixels[i] = pixels[i+2];
		pixels[i+2] = tmp;
	}

	pixcopy = new GLubyte[texturesize];
	GLubyte *ptr1 = pixels;
	GLubyte *ptr2 = pixcopy;

	for( i = 0; i < texturesize; ++i )
		*ptr2++ = *ptr1++;

	return true;
}

void reshape( int w, int h)
{
  W = w;
  H = h;
  glViewport(0, 0, w, h);
  glMatrixMode(GL_PROJECTION);
  glLoadIdentity();
  glOrtho(0.0f, (GLfloat) W, 0.0f, (GLfloat) H, -1.0f, 1.0f);
  glMatrixMode(GL_MODELVIEW);
}

void display(void)
{
  glRasterPos2i( 0, 0 );
  glDrawPixels( PixmapWidth, PixmapHeight, GL_RGB, GL_UNSIGNED_BYTE, pixels);
  glFlush();
}

void restore()
{
 	GLubyte *ptr1 = pixels;
	GLubyte *ptr2 = pixcopy;

	for( int i = 0; i < texturesize; ++i )
		*ptr1++ = *ptr2++;

    display();
}

void grayscale()
{
	cout << "clicked grayscale" << endl;
 	GLubyte *ptr1 = pixels;
	GLubyte *ptr2 = pixcopy;

	for( int i = 0; i < texturesize; i+=3 ) {
		int grayScale = *(ptr2+i)*0.3 + *(ptr2+i+1)*0.59f + *(ptr2+i+2)*0.11f;
		*(ptr1+i) = (GLubyte)grayScale;
		*(ptr1+i+1) = (GLubyte)grayScale;
		*(ptr1+i+2) = (GLubyte)grayScale;
	}

    display();
}

void ordered_dither_1()
{
  int grid1[3*6] = {
    9, 7, 8, 10, 12, 11,
    6, 1, 2, 13, 18, 17,
    5, 4, 3, 14, 15, 16,
  };
  int grid2[3*6] = {
    10, 12, 11, 9, 7, 8,
    13, 18, 17, 6, 1, 2,
    14, 15, 16, 5, 4, 3,
  };
  	cout << "clicked ordered dither 1" << endl;
 	GLubyte *ptr1 = pixels;
	GLubyte *ptr2 = pixcopy;

	for( int i = 0; i < texturesize; i+=3 ) {
		int grayScale = *(ptr2+i)*0.3 + *(ptr2+i+1)*0.59f + *(ptr2+i+2)*0.11f;
		int row = i/3/PixmapWidth;
		int col = i/3%PixmapWidth;
		int *pGrid;
		if (row/3%2 == 0) pGrid = grid1;
		else pGrid = grid2;
		if (grayScale*18/255 > pGrid[(row%3)*6+(col%6)]) { // pGrid[row%3][col%6]
		  grayScale = 255;
		} else {
		  grayScale = 0;
		}
		*(ptr1+i) = (GLubyte)grayScale;
		*(ptr1+i+1) = (GLubyte)grayScale;
		*(ptr1+i+2) = (GLubyte)grayScale;
	}

    display();
}

void ordered_dither_2()
{
  int grid1[4*8] = {
    14, 12, 13, 16, 19, 21, 20, 17,
    5, 4, 3, 10, 28, 29, 30, 23,
    6, 1, 2, 11, 27, 32, 31, 22,
    9, 7, 8, 15, 24, 26, 25, 18,
  };
  int grid2[4*8] = {
    19, 21, 20, 17, 14, 12, 13, 16,
    28, 29, 30, 23, 5, 4, 3, 10,
    27, 32, 31, 22, 6, 1, 2, 11,
    24, 26, 25, 18, 9, 7, 8, 15,
  };
	cout << "clicked ordered dither 2" << endl;
 	GLubyte *ptr1 = pixels;
	GLubyte *ptr2 = pixcopy;

	for( int i = 0; i < texturesize; i+=3 ) {
		int grayScale = *(ptr2+i)*0.3 + *(ptr2+i+1)*0.59f + *(ptr2+i+2)*0.11f;
		int row = i/3/PixmapWidth;
		int col = i/3%PixmapWidth;
		int *pGrid;
		if (row/3%2 == 0) pGrid = grid1;
		else pGrid = grid2;
		if (grayScale*32/255 > pGrid[(row%4)*8+(col%8)]) { // pGrid[row%4][col%8]
		  grayScale = 255;
		} else {
		  grayScale = 0;
		}
		*(ptr1+i) = (GLubyte)grayScale;
		*(ptr1+i+1) = (GLubyte)grayScale;
		*(ptr1+i+2) = (GLubyte)grayScale;
	}
	display();
}

void set_pixel_gray_scale(GLubyte *ptr1, int grayScale)
{
	if (grayScale > 255) grayScale = 255;
	else if (grayScale < 0) grayScale = 0;
	*(ptr1) = (GLubyte)grayScale;
	*(ptr1+1) = (GLubyte)grayScale;
	*(ptr1+2) = (GLubyte)grayScale;
}

void error_diffusion()
/* black and white version */
{
	cout << "clicked error diffusion" << endl;
 	GLubyte *ptr1 = pixels;
	GLubyte *ptr2 = pixcopy;
	GLubyte *line1, *line2;

	for( int i = 0; i < texturesize; i+=3 ) {
		int grayScale = *(ptr2+i)*0.3 + *(ptr2+i+1)*0.59f + *(ptr2+i+2)*0.11f;
		set_pixel_gray_scale(ptr1+i, grayScale);
	}

	for( int i = 0; i < PixmapHeight; i++ ) {
		for( int j = 0; j < PixmapWidth; j++ ) {
			int idx = (i*PixmapWidth+j)*3;
			line1 = ptr1+idx;
			line2 = ptr1+idx+PixmapWidth*3;
			int grayScale = *line1;
			int error;
			if (grayScale > 128) {
				error = grayScale - 255;
				grayScale = 255;
			} else {
				error = grayScale;
				grayScale = 0;
			}
			set_pixel_gray_scale(line1, grayScale);
			// set right
			if (j < PixmapWidth-1) {
				set_pixel_gray_scale(line1+3, *(line1+3) + error*7/16);
			}
			// set left bottom
			if (j > 0 && i < PixmapHeight-1) {
				set_pixel_gray_scale(line2-3, *(line2-3) + error*3/16);
			}
			// set bottom
			if (i < PixmapHeight-1) {
				set_pixel_gray_scale(line2, *(line2) + error*5/16);
			}
			// set right bottom
			if (j < PixmapWidth-1 && i < PixmapHeight-1) {
				set_pixel_gray_scale(line2+3, *(line2+3) + error*1/16);
			}
		}
	}

    display();
}

void line_halftone()
{
/*	int grid[6*6] = {
		36, 34, 32, 31, 33, 35,
		24, 22, 20, 19, 21, 23,
		12, 10, 8, 7, 9, 11,
		6, 4, 2, 1, 3, 5,
		18, 16, 14, 13, 15, 17,
		30, 28, 26, 25, 27, 29,
	};*/
	int grid[6*6] = {
		36, 24, 12, 6, 18, 30,
		34, 22, 10, 4, 16, 28,
		32, 20, 8, 2, 14, 26,
		31, 19, 7, 1, 13, 25,
		33, 21, 9, 3, 15, 27,
		35, 23, 11, 5, 17, 29,
	};
	cout << "clicked line halftone" << endl;
 	GLubyte *ptr1 = pixels;
	GLubyte *ptr2 = pixcopy;

	for( int i = 0; i < texturesize; i+=3 ) {
		int grayScale = *(ptr2+i)*0.3 + *(ptr2+i+1)*0.59f + *(ptr2+i+2)*0.11f;
		int row = i/3/PixmapWidth;
		int col = i/3%PixmapWidth;
		if (grayScale*36/255 > grid[(row%6)*6+(col%6)]) { // grid[row%6][col%6]
		  grayScale = 255;
		} else {
		  grayScale = 0;
		}
		*(ptr1+i) = (GLubyte)grayScale;
		*(ptr1+i+1) = (GLubyte)grayScale;
		*(ptr1+i+2) = (GLubyte)grayScale;
	}
	display();
}


static GLubyte paletteRGB[5][3] = {
  {255, 0, 0},
  {0, 255, 0},
  {0, 0, 255},
  {0, 0, 0},
  {255, 255, 255},
};
static GLubyte paletteCMY[5][3] = {
  {0, 255, 255},
  {255, 0, 255},
  {255, 255, 0},
  {0, 0, 0},
  {255, 255, 255},
};
//#include "math.h"
//// return the index of color in palette above
//int find_nearest_colour(GLubyte inR, GLubyte inG, GLubyte inB, int &error)
//{
//  error = 255*255*3;
//  int idx;
//  for (int i = 0; i < 5; i++) {
//    int diffR = inR - palette[i][0];
//    int diffG = inG - palette[i][1];
//    int diffB = inB - palette[i][2];
//    int dist = diffR*diffR + diffG*diffG + diffB*diffB;
//    if (dist < error) {
//      error = dist;
//      idx = i;
//    }
//  }
//  error = sqrt((float)error);
//  return idx;
//}
// return the index of color in palette above
int find_nearest_colour(GLubyte inR, GLubyte inG, GLubyte inB, int &errorR, int &errorG, int &errorB)
{
  int min = 255*255*3;
  int idx = 0;
  for (int i = 0; i < 5; i++) {
    int diffR = inR - paletteCMY[i][0];
    int diffG = inG - paletteCMY[i][1];
    int diffB = inB - paletteCMY[i][2];
    int dist = diffR*diffR + diffG*diffG + diffB*diffB;
    if (dist < min) {
      min = dist;
      errorR = diffR;
      errorG = diffG;
      errorB = diffB;
      idx = i;
    }
  }
  return idx;
}

void set_pixel_color(GLubyte *ptr1, GLubyte r, GLubyte g, GLubyte b)
{
	if (r > 255) r = 255;
	else if (r < 0) r = 0;
	if (g > 255) g = 255;
	else if (g < 0) g = 0;
	if (b > 255) b = 255;
	else if (b < 0) b = 0;
	*(ptr1) = r;
	*(ptr1+1) = g;
	*(ptr1+2) = b;
}

void colour_error_diffusion()

/* colour version
   that uses 5 'colours' - red, green, blue, black white
   Try it also five different colors.
*/
{
	cout << "clicked error diffusion" << endl;
 	GLubyte *ptr1 = pixels;
	GLubyte *ptr2 = pixcopy;
	GLubyte *line1, *line2;

	// restore image first
	for( int i = 0; i < texturesize; ++i )
		*(ptr1+i) = *(ptr2+i);

	for( int i = 0; i < PixmapHeight; i++ ) {
		for( int j = 0; j < PixmapWidth; j++ ) {
			int idx = (i*PixmapWidth+j)*3;
			line1 = ptr1+idx;
			line2 = ptr1+idx+PixmapWidth*3;
			int errorR, errorG, errorB;
			int idx_palette = find_nearest_colour(*line1, *(line1+1), *(line1+2), errorR, errorG, errorB);
			set_pixel_color(line1, paletteCMY[idx_palette][0], paletteCMY[idx_palette][1], paletteCMY[idx_palette][2]);
			// set right
			if (j < PixmapWidth-1) {
				set_pixel_color(line1+3, *(line1+3) + errorR*7/16, *(line1+4) + errorG*7/16, *(line1+5) + errorB*7/16);
			}
			// set left bottom
			if (j > 0 && i < PixmapHeight-1) {
				set_pixel_color(line2-3, *(line2-3) + errorR*3/16, *(line2-2) + errorG*3/16, *(line2-1) + errorB*3/16);
			}
			// set bottom
			if (i < PixmapHeight-1) {
				set_pixel_color(line2, *(line2) + errorR*5/16, *(line2+1) + errorG*5/16, *(line2+2) + errorB*5/16);
			}
			// set right bottom
			if (j < PixmapWidth-1 && i < PixmapHeight-1) {
				set_pixel_color(line2+3, *(line2+3) + errorR*1/16, *(line2+4) + errorG*1/16, *(line2+5) + errorB*1/16);
			}
		}
	}

    display();
}

void transform(GLubyte *ptr, int row, int col)
{
	int sum = 0;
	if (row > 0 && col > 0) {
		sum += *(ptr-PixmapWidth*3-3)*0.125;
	}
	if (row > 0) {
		sum += *(ptr-PixmapWidth*3)*0.125;
	}
	if (row > 0 && col < PixmapWidth-1) {
		sum += *(ptr-PixmapWidth*3+3)*0.125;
	}
	if (col > 0) {
		sum += *(ptr-3)*0.125;
	}
	sum -= *(ptr);
	if (col < PixmapWidth-1) {
		sum += *(ptr+3)*0.125;
	}
	if (row < PixmapHeight-1 && col > 0) {
		sum += *(ptr+PixmapWidth*3-3)*0.125;
	}
	if (row < PixmapHeight-1) {
		sum += *(ptr+PixmapWidth*3)*0.125;
	}
	if (row < PixmapHeight-1 && col < PixmapWidth-1) {
		sum += *(ptr+PixmapWidth*3+3)*0.125;
	}
	sum = *(ptr) - 2*sum;
	if (sum > 255) sum = 255;
	else if (sum < 0) sum = 0;
	*(ptr) = (GLubyte)sum;
}

void sharpen()
{
//0.125	 0.125	 0.125
//0.125	 -1.000	 0.125
//0.125	 0.125	 0.125
	cout << "clicked sharpen edges" << endl;
 	GLubyte *ptr1 = pixels;
	GLubyte *ptr2 = pixcopy;

	// restore image first
	for( int i = 0; i < texturesize; ++i )
		*(ptr1+i) = *(ptr2+i);

	for( int i = 0; i < PixmapHeight; i++ ) {
		for( int j = 0; j < PixmapWidth; j++ ) {
			int idx = (i*PixmapWidth+j)*3;
			transform(ptr1+idx, i, j); // transform red
			transform(ptr1+idx+1, i, j); // transform green
			transform(ptr1+idx+2, i, j); // transform blue
		}
	}

	display();
}

void disperse()
{
	int grid[4*4] = {
		2, 16, 3, 13,
		10, 6, 11, 7,
		4, 14, 1, 15,
		12, 8, 9, 5,
	};
	cout << "clicked dispersed dot" << endl;
 	GLubyte *ptr1 = pixels;
	GLubyte *ptr2 = pixcopy;

	for( int i = 0; i < texturesize; i+=3 ) {
		int grayScale = *(ptr2+i)*0.3 + *(ptr2+i+1)*0.59f + *(ptr2+i+2)*0.11f;
		int row = i/3/PixmapWidth;
		int col = i/3%PixmapWidth;
		if (grayScale*16/255 > grid[(row%4)*4+(col%4)]) { // grid[row%4][col%4]
		  grayScale = 255;
		} else {
		  grayScale = 0;
		}
		*(ptr1+i) = (GLubyte)grayScale;
		*(ptr1+i+1) = (GLubyte)grayScale;
		*(ptr1+i+2) = (GLubyte)grayScale;
	}
	display();
}

void special()
{
	// Colour inversion and change rgb->bgr and grayscale in circle
	cout << "clicked special" << endl;
 	GLubyte *ptr1 = pixels;
	GLubyte *ptr2 = pixcopy;

	int center_row = PixmapHeight/2;
	int center_col = PixmapWidth/2;
	//cout << "center: " << center_row << ", " << center_col << endl;
	for( int i = 0; i < texturesize; i+=3 ) {
		int row = i/3/PixmapWidth;
		int col = i/3%PixmapWidth;
		if ((row-center_row)*(row-center_row) + (col-center_col)*(col-center_col) > 40000) {
			int r = *(ptr2+i);
			int g = *(ptr2+i+1);
			int b = *(ptr2+i+2);
			*(ptr1+i) = 255 - b;
			*(ptr1+i+1) = 255 - g;
			*(ptr1+i+2) = 255 - r;
		} else {
			int grayScale = *(ptr2+i)*0.3 + *(ptr2+i+1)*0.59f + *(ptr2+i+2)*0.11f;
			*(ptr1+i) = (GLubyte)grayScale;
			*(ptr1+i+1) = (GLubyte)grayScale;
			*(ptr1+i+2) = (GLubyte)grayScale;
		}
	}
	display();
}

int halftone_type = 1;
int old_halftone_type = 1;


void mouseFunc(int, int state, int, int)
{

	if ( state != GLUT_DOWN )
		return;
  cout << PixmapWidth << "  " << PixmapHeight << endl;

  cout << "     1. original colour\n";
  cout << "     2. grayscale\n";
  cout << "     3. ordered dither 1\n";
  cout << "     4. ordered dither 2\n";
  cout << "     5. error diffusion\n";
  cout << "     6. line halftone\n";
  cout << "     7. colour error diffusion\n";
  cout << "     8. sharpen edges\n";
  cout << "     9. dispersed dot\n";
  cout << "    10. special\n";

  cin >> halftone_type;
  cout << " you selected " << halftone_type << endl;
  if (halftone_type == old_halftone_type)
    return;

  switch(halftone_type) {
  case 1:
    restore();
    break;
  case 2:
    grayscale();
    break;
  case 3:
    ordered_dither_1();
    break;
  case 4:
    ordered_dither_2();
    break;
  case 5:
    error_diffusion();
    break;
  case 6:
    line_halftone();
    break;
  case 7:
    colour_error_diffusion();
    break;
  case 8:
    sharpen();
    break;
  case 9:
    disperse();
    break;
  case 10:
    special();
    break;
  default:
    break;
  }

  old_halftone_type = halftone_type;

}

void
main( int argc, char **argv)
{
  char c;

  if (!initialize())
	  return;
  glutInitDisplayMode ( GLUT_RGB );
  int windowHandle = glutCreateWindow( "TME 3: Image Processing" );
  glutSetWindow( windowHandle );
  glutPositionWindow(250, 250);
  glutReshapeWindow( PixmapWidth, PixmapHeight );
  glutMouseFunc( mouseFunc );
  glutReshapeFunc(reshape);
  reshape(PixmapWidth, PixmapHeight);
  cout << PixmapWidth << ", " << PixmapHeight << endl;
  glFlush();
  glClearColor(0.0f, 0.0f, 0.0f, 1.0f);
  glutDisplayFunc(display);
  glutMainLoop();
}