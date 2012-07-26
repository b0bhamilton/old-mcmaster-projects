//Robert Hamilton
//Athabasca University: COMP 390
//TME 2pt 1

// starter.cpp : Defines the entry point for the console application.
//
#include "stdio.h"
#include "gl/glut.h"
#include "math.h"
#include <stdlib.h>

// Constant definitions for Menus
#define AMB 1
#define DIF 2
#define SPC 3

#define GLD 1
#define SVR 2
#define CPR 3

// Pop up menu identifiers
int mainMenu, elementsMenu, materialsMenu, lightMenu_one,lightMenu_two, lightMenu_three;

//Ambient is always 1
int difflag =0;
int specflag=0;

// menu status
int menuFlag = 0;

	// lights
GLfloat white_light[] = { 1.0, 1.0, 1.0, 1.0 };
GLfloat green_light[] = { 0.2, 1.0, 0.2, 1.0 };
GLfloat red_light[] = { 1.0, 0.4, 0.4, 1.0 };
GLfloat blue_light[] = { 0.2, 0.2, 1.0, 1.0 };
GLfloat no_light[] = { 0.0, 0.0, 0.0, 1.0 };

//Light position at source (1,1,0)
//behind Viewer
GLfloat light_position1[] = { 0.0, 0.0, 2.0, 0.0 };
//Upper Right corner
GLfloat light_position2[] = { 20.0, 12.0, 30.0, 0.0 };
//Upper Left
GLfloat light_position3[] = { -20.0, 12.0, 30.0, 0.0 };

	
GLfloat lmodel_ambient[] = { 0.9, 0.9, 0.9, 1.0 };  // step 5

	 // gold
 GLfloat gold_ambient[] = { 0.24725, 0.1995, 0.0745, 1.0 };
 GLfloat gold_diffuse[] = { 0.75164, 0.60648, 0.22658, 1.0 };
 GLfloat gold_specular[] = { 0.628281, 0.555802, 0.366065, 1.0 };
 GLfloat gold_shininess = { 51.2 };
 // silver
 GLfloat silver_ambient[] = { 0.19225, 0.19225, 0.19225, 1.0 };
 GLfloat silver_diffuse[] = { 0.50754, 0.50754, 0.50754, 1.0 };
 GLfloat silver_specular[] = { 0.508273, 0.508273, 0.508273, 1.0 };
 GLfloat silver_shininess = { 51.2 };
 // copper
 GLfloat copper_ambient[] = { 0.2295, 0.08825, 0.0275, 1.0 };
 GLfloat copper_diffuse[] = { 0.5508, 0.2118, 0.066, 1.0 };
 GLfloat copper_specular[] = { 0.580594, 0.223257, 0.0695701, 1.0 };
 GLfloat copper_shininess = { 51.2 };

//float position[4] = {1.5,0,0, 0};

float amb[4] = {1.0, 1, 1, 1};
float diff[4] = {0.75,0.75,0.75, 1};
float spec[4] = {1,1,1, 1};

float offamb[4] = {0, 0, 0, 0};
float offdiff[4] = {0,0,0, 0};
float offspec[4] = {0,0,0, 0};

float amb2[4] = {1.0, 1, 1, 1};
float diff2[4] = {0.75,0.75,0.75, 1};
float spec2[4] = {1,1,1, 1};

float offamb2[4] = {0, 0, 0, 0};
float offdiff2[4] = {0,0,0, 0};
float offspec2[4] = {0,0,0, 0};

float amb3[4] = {1.0, 1, 1, 1};
float diff3[4] = {0.75,0.75,0.75, 1};
float spec3[4] = {1,1,1, 1};

float offamb3[4] = {0, 0, 0, 0};
float offdiff3[4] = {0,0,0, 0};
float offspec3[4] = {0,0,0, 0};

	//
GLfloat position = 5.0f;
GLfloat angle = 0.0f;

void initialize(void) {

	// set background color
	glClearColor(0.5, 0.5, 0.5, 0.0);

	// enable depth test
	glEnable(GL_DEPTH_TEST);

	// set lights
	glLightfv(GL_LIGHT0, GL_POSITION,  light_position1); 
	glLightfv(GL_LIGHT1, GL_POSITION,  light_position2);
	glLightfv(GL_LIGHT2, GL_POSITION,  light_position3);

	


	glLightModelfv(GL_LIGHT_MODEL_AMBIENT, lmodel_ambient);  // step 4

	glEnable(GL_LIGHTING); 
	

}


void scene(void) {
	glPushMatrix();
		//glTranslatef(1.0, 1.0, 0.0);
		glutSolidSphere(1.5, 42, 50);
	glPopMatrix();
}

// -----------------------------------
//             MENUS
// -----------------------------------

void processMenuStatus(int status, int x, int y) {

	if (status == GLUT_MENU_IN_USE)
		menuFlag = 1;
	else
		menuFlag = 0;
}

void processMainMenu(int option) {
	switch (option) {
	case 0: exit(0);
	};
}


void processElementsMenu (int option) {
	switch (option) {
	case AMB:
	//glLightfv(GL_LIGHT1, GL_POSITION, position);
	glLightfv(GL_LIGHT0, GL_DIFFUSE, offdiff);
	glLightfv(GL_LIGHT0, GL_SPECULAR, offspec);
	glLightfv(GL_LIGHT0, GL_AMBIENT, amb);

	glLightfv(GL_LIGHT1, GL_DIFFUSE, offdiff2);
	glLightfv(GL_LIGHT1, GL_SPECULAR, offspec2);
	glLightfv(GL_LIGHT1, GL_AMBIENT, amb2);

	glLightfv(GL_LIGHT2, GL_DIFFUSE, offdiff3);
	glLightfv(GL_LIGHT2, GL_SPECULAR, offspec3);
	glLightfv(GL_LIGHT2, GL_AMBIENT, amb3);

	difflag =0;
	specflag=0;
	break;
	
	case DIF: 
	glLightfv(GL_LIGHT0, GL_SPECULAR, offspec);
	glLightfv(GL_LIGHT0, GL_AMBIENT, amb);
	glLightfv(GL_LIGHT0, GL_DIFFUSE, diff);

	glLightfv(GL_LIGHT1, GL_SPECULAR, offspec2);
	glLightfv(GL_LIGHT1, GL_AMBIENT, amb2);
	glLightfv(GL_LIGHT1, GL_DIFFUSE, diff2);

	glLightfv(GL_LIGHT2, GL_SPECULAR, offspec3);
	glLightfv(GL_LIGHT2, GL_AMBIENT, amb3);
	glLightfv(GL_LIGHT2, GL_DIFFUSE, diff3);

	difflag =1;
	specflag=0;
	break;

	case SPC:
	glLightfv(GL_LIGHT0, GL_DIFFUSE, diff);
	glLightfv(GL_LIGHT0, GL_AMBIENT, amb);
	glLightfv(GL_LIGHT0, GL_SPECULAR, spec);

	glLightfv(GL_LIGHT1, GL_DIFFUSE, diff2);
	glLightfv(GL_LIGHT1, GL_AMBIENT, amb2);
	glLightfv(GL_LIGHT1, GL_SPECULAR, spec2);

	glLightfv(GL_LIGHT2, GL_DIFFUSE, diff3);
	glLightfv(GL_LIGHT2, GL_AMBIENT, amb3);
	glLightfv(GL_LIGHT2, GL_SPECULAR, spec3);

	difflag =1;
	specflag=1;
	break;
	}
	glutPostRedisplay();
}

//swaps contents of array b into array a
void putarray (GLfloat a[], GLfloat b[]) {
	int x;
	for (x=0; x< sizeof(b); x++ ) {
		a[x] = b[x];
	}
}

void processMaterialsMenu(int option) {
switch (option) {
	case GLD: 
	glMaterialfv(GL_FRONT_AND_BACK, GL_AMBIENT,gold_ambient);
	glMaterialfv(GL_FRONT_AND_BACK, GL_DIFFUSE, gold_diffuse);
	glMaterialfv(GL_FRONT_AND_BACK, GL_SPECULAR, gold_specular);
	glMaterialf(GL_FRONT_AND_BACK, GL_SHININESS, gold_shininess);
	break;
	case SVR:
			glMaterialfv(GL_FRONT_AND_BACK, GL_AMBIENT,silver_ambient);
	glMaterialfv(GL_FRONT_AND_BACK, GL_DIFFUSE, silver_diffuse);
	glMaterialfv(GL_FRONT_AND_BACK, GL_SPECULAR, silver_specular);
	glMaterialf(GL_FRONT_AND_BACK, GL_SHININESS, silver_shininess);
		break;
	case CPR:
		glMaterialfv(GL_FRONT_AND_BACK, GL_AMBIENT,copper_ambient);
	glMaterialfv(GL_FRONT_AND_BACK, GL_DIFFUSE, copper_diffuse);
	glMaterialfv(GL_FRONT_AND_BACK, GL_SPECULAR, copper_specular);
	glMaterialf(GL_FRONT_AND_BACK, GL_SHININESS, copper_shininess);
		break;
}
glutPostRedisplay();
}

void processLightMenu_one(int option) {
switch (option){
case 0:
	//off
	glDisable(GL_LIGHT0); 
	//you could alternatively use the color code for no_light
	break;
case 1:
	//white
	putarray(amb,white_light);
	putarray(diff,white_light);
	putarray(spec,white_light);

	glEnable(GL_LIGHT0); 
	if (specflag== 1) {
		glLightfv(GL_LIGHT0, GL_SPECULAR, spec);
	}else if (specflag==0) {
		glLightfv(GL_LIGHT0, GL_SPECULAR, offspec);
	};

	if (difflag ==1) {
		glLightfv(GL_LIGHT0, GL_DIFFUSE, diff);
	}else if(difflag ==0) {
		glLightfv(GL_LIGHT0, GL_DIFFUSE, offdiff);
	}
	
	glLightfv(GL_LIGHT0, GL_AMBIENT, amb);
	break;
case 2:
	//red
	putarray(amb,red_light);
	putarray(diff,red_light);
	putarray(spec,red_light);

	glEnable(GL_LIGHT0); 
	if (specflag== 1) {
		glLightfv(GL_LIGHT0, GL_SPECULAR, spec);
	}else if (specflag==0) {
		glLightfv(GL_LIGHT0, GL_SPECULAR, offspec);
	};

	if (difflag ==1) {
		glLightfv(GL_LIGHT0, GL_DIFFUSE, diff);
	}else if(difflag ==0) {
		glLightfv(GL_LIGHT0, GL_DIFFUSE, offdiff);
	}
	
	glLightfv(GL_LIGHT0, GL_AMBIENT, amb);
	break;
case 3:
	//blue
	putarray(amb,blue_light);
	putarray(diff,blue_light);
	putarray(spec,blue_light);

	glEnable(GL_LIGHT0); 
	if (specflag== 1) {
		glLightfv(GL_LIGHT0, GL_SPECULAR, spec);
	}else if (specflag==0) {
		glLightfv(GL_LIGHT0, GL_SPECULAR, offspec);
	};

	if (difflag ==1) {
		glLightfv(GL_LIGHT0, GL_DIFFUSE, diff);
	}else if(difflag ==0) {
		glLightfv(GL_LIGHT0, GL_DIFFUSE, offdiff);
	}
	
	glLightfv(GL_LIGHT0, GL_AMBIENT, amb);

	break;
case 4:
	//green
	putarray(amb,green_light);
	putarray(diff,green_light);
	putarray(spec,green_light);

	glEnable(GL_LIGHT0); 
	if (specflag== 1) {
		glLightfv(GL_LIGHT0, GL_SPECULAR, spec);
	}else if (specflag==0) {
		glLightfv(GL_LIGHT0, GL_SPECULAR, offspec);
	};

	if (difflag ==1) {
		glLightfv(GL_LIGHT0, GL_DIFFUSE, diff);
	}else if(difflag ==0) {
		glLightfv(GL_LIGHT0, GL_DIFFUSE, offdiff);
	}
	
	glLightfv(GL_LIGHT0, GL_AMBIENT, amb);
	break;
};

glutPostRedisplay();
}



void processLightMenu_two(int option) {
switch (option){
case 0:
	//off
	glDisable(GL_LIGHT1); 
	//you could alternatively use the color code for no_light
	break;
case 1:
	//white
	putarray(amb2,white_light);
	putarray(diff2,white_light);
	putarray(spec2,white_light);

	glEnable(GL_LIGHT1); 
	if (specflag== 1) {
		glLightfv(GL_LIGHT1, GL_SPECULAR, spec2);
	}else if (specflag==0) {
		glLightfv(GL_LIGHT1, GL_SPECULAR, offspec2);
	};

	if (difflag ==1) {
		glLightfv(GL_LIGHT1, GL_DIFFUSE, diff2);
	}else if(difflag ==0) {
		glLightfv(GL_LIGHT1, GL_DIFFUSE, offdiff2);
	}
	
	glLightfv(GL_LIGHT1, GL_AMBIENT, amb2);
	break;
case 2:
	//red
	putarray(amb2,red_light);
	putarray(diff2,red_light);
	putarray(spec2,red_light);

	glEnable(GL_LIGHT1); 
	if (specflag== 1) {
		glLightfv(GL_LIGHT1, GL_SPECULAR, spec2);
	}else if (specflag==0) {
		glLightfv(GL_LIGHT1, GL_SPECULAR, offspec2);
	};

	if (difflag ==1) {
		glLightfv(GL_LIGHT1, GL_DIFFUSE, diff2);
	}else if(difflag ==0) {
		glLightfv(GL_LIGHT1, GL_DIFFUSE, offdiff2);
	}
	
	glLightfv(GL_LIGHT1, GL_AMBIENT, amb2);
	break;
case 3:
	//blue
	putarray(amb2,blue_light);
	putarray(diff2,blue_light);
	putarray(spec2,blue_light);

	glEnable(GL_LIGHT1); 
	if (specflag== 1) {
		glLightfv(GL_LIGHT1, GL_SPECULAR, spec2);
	}else if (specflag==0) {
		glLightfv(GL_LIGHT1, GL_SPECULAR, offspec2);
	};

	if (difflag ==1) {
		glLightfv(GL_LIGHT1, GL_DIFFUSE, diff2);
	}else if(difflag ==0) {
		glLightfv(GL_LIGHT1, GL_DIFFUSE, offdiff2);
	}
	
	glLightfv(GL_LIGHT1, GL_AMBIENT, amb2);

	break;
case 4:
	//green
	putarray(amb2,green_light);
	putarray(diff2,green_light);
	putarray(spec2,green_light);

	glEnable(GL_LIGHT1); 
	if (specflag== 1) {
		glLightfv(GL_LIGHT1, GL_SPECULAR, spec2);
	}else if (specflag==0) {
		glLightfv(GL_LIGHT1, GL_SPECULAR, offspec2);
	};

	if (difflag ==1) {
		glLightfv(GL_LIGHT1, GL_DIFFUSE, diff2);
	}else if(difflag ==0) {
		glLightfv(GL_LIGHT1, GL_DIFFUSE, offdiff2);
	}
	
	glLightfv(GL_LIGHT1, GL_AMBIENT, amb2);
	break;
};

glutPostRedisplay();
}



void processLightMenu_three(int option) {
switch (option){
case 0:
	//off
	glDisable(GL_LIGHT2); 
	//you could alternatively use the color code for no_light
	break;
case 1:
	//white
	putarray(amb3,white_light);
	putarray(diff3,white_light);
	putarray(spec3,white_light);

	glEnable(GL_LIGHT2); 
	if (specflag== 1) {
		glLightfv(GL_LIGHT2, GL_SPECULAR, spec3);
	}else if (specflag==0) {
		glLightfv(GL_LIGHT2, GL_SPECULAR, offspec3);
	};

	if (difflag ==1) {
		glLightfv(GL_LIGHT2, GL_DIFFUSE, diff3);
	}else if(difflag ==0) {
		glLightfv(GL_LIGHT2, GL_DIFFUSE, offdiff3);
	}
	
	glLightfv(GL_LIGHT2, GL_AMBIENT, amb3);
	break;
case 2:
	//red
	putarray(amb3,red_light);
	putarray(diff3,red_light);
	putarray(spec3,red_light);

	glEnable(GL_LIGHT2); 
	if (specflag== 1) {
		glLightfv(GL_LIGHT2, GL_SPECULAR, spec3);
	}else if (specflag==0) {
		glLightfv(GL_LIGHT2, GL_SPECULAR, offspec3);
	};

	if (difflag ==1) {
		glLightfv(GL_LIGHT2, GL_DIFFUSE, diff3);
	}else if(difflag ==0) {
		glLightfv(GL_LIGHT2, GL_DIFFUSE, offdiff3);
	}
	
	glLightfv(GL_LIGHT2, GL_AMBIENT, amb3);
	break;
case 3:
	//blue
	putarray(amb3,blue_light);
	putarray(diff3,blue_light);
	putarray(spec3,blue_light);

	glEnable(GL_LIGHT2); 
	if (specflag== 1) {
		glLightfv(GL_LIGHT2, GL_SPECULAR, spec3);
	}else if (specflag==0) {
		glLightfv(GL_LIGHT2, GL_SPECULAR, offspec3);
	};

	if (difflag ==1) {
		glLightfv(GL_LIGHT2, GL_DIFFUSE, diff3);
	}else if(difflag ==0) {
		glLightfv(GL_LIGHT2, GL_DIFFUSE, offdiff3);
	}
	
	glLightfv(GL_LIGHT2, GL_AMBIENT, amb3);
	break;
case 4:
	//green
	putarray(amb3,green_light);
	putarray(diff3,green_light);
	putarray(spec3,green_light);

	glEnable(GL_LIGHT2); 
	if (specflag== 1) {
		glLightfv(GL_LIGHT2, GL_SPECULAR, spec3);
	}else if (specflag==0) {
		glLightfv(GL_LIGHT2, GL_SPECULAR, offspec3);
	};

	if (difflag ==1) {
		glLightfv(GL_LIGHT2, GL_DIFFUSE, diff3);
	}else if(difflag ==0) {
		glLightfv(GL_LIGHT2, GL_DIFFUSE, offdiff3);
	}
	
	glLightfv(GL_LIGHT2, GL_AMBIENT, amb3);
	break;
};

glutPostRedisplay();
}




void createPopupMenus() {

	lightMenu_one = glutCreateMenu(processLightMenu_one);
	glutAddMenuEntry("Off",0);
	glutAddMenuEntry("White Light",1);
	glutAddMenuEntry("Red Light",2);
	glutAddMenuEntry("Blue Light",3);
	glutAddMenuEntry("Green Light",4);
	
	
	lightMenu_two = glutCreateMenu(processLightMenu_two);
	glutAddMenuEntry("Off",0);
	glutAddMenuEntry("White Light",1);
	glutAddMenuEntry("Red Light",2);
	glutAddMenuEntry("Blue Light",3);
	glutAddMenuEntry("Green Light",4);

	
	lightMenu_three = glutCreateMenu(processLightMenu_three);
	glutAddMenuEntry("Off",0);
	glutAddMenuEntry("White Light",1);
	glutAddMenuEntry("Red Light",2);
	glutAddMenuEntry("Blue Light",3);
	glutAddMenuEntry("Green Light",4);
	
	materialsMenu = glutCreateMenu (processMaterialsMenu);
	glutAddMenuEntry("Gold",GLD);
	glutAddMenuEntry("Silver",SVR);
	glutAddMenuEntry("Copper",CPR);


	elementsMenu = glutCreateMenu (processElementsMenu);
	glutAddMenuEntry("Ambient only",AMB);
	glutAddMenuEntry("Ambient and Diffuse",DIF);
	glutAddMenuEntry("Ambient, Diffuse and Specular",SPC);

	mainMenu = glutCreateMenu(processMainMenu);

	glutAddSubMenu("Light 1", lightMenu_one);
	glutAddSubMenu("Light 2", lightMenu_two);
	glutAddSubMenu("Light 3", lightMenu_three);
	glutAddSubMenu("Material Properties", materialsMenu);
	glutAddSubMenu("Light Elements", elementsMenu);
	glutAddMenuEntry("Quit",0);
	// attach the menu to the right button
	glutAttachMenu(GLUT_RIGHT_BUTTON);

	// this will allow us to know if the menu is active
	glutMenuStatusFunc(processMenuStatus);
}


void display(void) {
	glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);
	glLoadIdentity();

	gluLookAt(0.0, 0.0, position, 0.0, 0.0, 0.0, 0.0, 1.0, 0.0);

	scene();

	glFlush();
	glutSwapBuffers();
}

void reshape(int w, int h) {
	glViewport(0, 0, (GLsizei) w, (GLsizei) h);
	glMatrixMode(GL_PROJECTION);
	glLoadIdentity();
	glFrustum(-1.0, 1.0, -1.0, 1.0, 1.5, 40.0);
	glMatrixMode(GL_MODELVIEW);
}

void main(int argc, char **argv)
{
	glutInit( &argc, argv );
	glutInitDisplayMode (GLUT_DOUBLE | GLUT_RGB | GLUT_DEPTH) ;
	glutInitWindowSize(500, 500);
  	glutInitWindowPosition(100, 100);

	int windowHandle = glutCreateWindow("Comp390 TME 2 pt 1");
	glutSetWindow(windowHandle);

	glutDisplayFunc( display );
	glutReshapeFunc( reshape );

	initialize();

	// init Menus
	createPopupMenus();

    glutMainLoop();
}