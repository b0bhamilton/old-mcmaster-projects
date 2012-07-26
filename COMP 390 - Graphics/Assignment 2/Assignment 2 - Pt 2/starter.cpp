//Robert Hamilton
//Athabasca University: COMP 390
//TME 2pt 2

// add light calculated by self
//
// eye is located at (0, 0, 0)
// 3 light sources, top-left (red), top-right (green), directly behind viewer (blue)
//

#include <stdlib.h>
#include "GL/glut.h"
#include "math.h"

#define RADIUS 12
#define STEP 10
#define POINTNUM (360/STEP)
#define CIRCLENUM 100
#define CYLINDER 1
#define PI 3.14
#define CYLINDER_Y -10
#define CYLINDER_Z -40

struct Point3D{
  float x;
  float y;
  float z;
};

struct Point3D curve[POINTNUM];

static void normalize_vector(float *v) {
  double d;
  d = sqrt((v[0]*v[0]) + (v[1]*v[1]) + (v[2]*v[2]));
  v[0] = v[0] / d;
  v[1] = v[1] / d;
  v[2] = v[2] / d;
}

static float dot(float *v1, float *v2) {
  return v1[0]*v2[0]+v1[1]*v2[1]+v1[2]*v2[2];
}

static void vector_multiply_num(float *v, float n, float *res) {
  res[0] = v[0] * n;
  res[1] = v[1] * n;
  res[2] = v[2] * n;
}

static void vector_add_vector(float *v1, float *v2, float *res) {
  res[0] = v1[0] + v2[0];
  res[1] = v1[1] + v2[1];
  res[2] = v1[2] + v2[2];
}

// calculate color value of one vertex from one light source
static void calc_light(Point3D vertex, float *normal, float *direction, float *ambient, float *diffuse, float *specular, float *color) {
  normalize_vector(normal);
  float light_dot_normal = dot(direction, normal);
//  if (light_dot_normal < 0)
//    light_dot_normal = 0;

  // calculate diffuse
  vector_multiply_num(diffuse, light_dot_normal, color);

  // calculate ambient
  vector_add_vector(color, ambient, color);

  if (light_dot_normal > 0) {
    // calculate specular, here we use Phong Model
    // get reflection vector first, formular: R = 2*N*dot(L, N)-L
    float reflection[3];
    vector_multiply_num(normal, -2*light_dot_normal, reflection);
    vector_add_vector(reflection, direction, reflection);
    vector_multiply_num(reflection, -1, reflection);
    normalize_vector(reflection);
    // get vector that from vertex to eye
    float vertex_to_eye[3] = {-vertex.x, -vertex.y, -vertex.z};
    normalize_vector(vertex_to_eye);
    float vertex_dot_reflection = dot(vertex_to_eye, reflection);
    if (vertex_dot_reflection < 0)
      vertex_dot_reflection = 0;
    float n = pow(vertex_dot_reflection, 20);
    // get specular, formular: IntensityOfSpecular = pow(dot(R, vertex_to_eye), shininess)*Ls
    float specular_color[3] = {0, 0, 0};
    vector_multiply_num(specular, n, specular_color);
    // add specular to final color
    vector_add_vector(color, specular_color, color);
  }
  //printf("%03f, %03f, %03f\n", color[0], color[1], color[2]);
}

// draw a circle on the specific y
GLvoid DrawCurve(float y) {
  float direction0[4], direction1[4], direction2[4];
  float ambient0[4], ambient1[4], ambient2[4];
  float diffuse0[4], diffuse1[4], diffuse2[4];
  float specular0[4], specular1[4], specular2[4];
  glGetLightfv(GL_LIGHT0, GL_POSITION, direction0);
  glGetLightfv(GL_LIGHT0, GL_AMBIENT, ambient0);
  glGetLightfv(GL_LIGHT0, GL_DIFFUSE, diffuse0);
  glGetLightfv(GL_LIGHT0, GL_SPECULAR, specular0);
  glGetLightfv(GL_LIGHT1, GL_POSITION, direction1);
  glGetLightfv(GL_LIGHT1, GL_AMBIENT, ambient1);
  glGetLightfv(GL_LIGHT1, GL_DIFFUSE, diffuse1);
  glGetLightfv(GL_LIGHT1, GL_SPECULAR, specular1);
  glGetLightfv(GL_LIGHT2, GL_POSITION, direction2);
  glGetLightfv(GL_LIGHT2, GL_AMBIENT, ambient2);
  glGetLightfv(GL_LIGHT2, GL_DIFFUSE, diffuse2);
  glGetLightfv(GL_LIGHT2, GL_SPECULAR, specular2);

  normalize_vector(direction0);
  normalize_vector(direction1);
  normalize_vector(direction2);
  //printf("diffuse0: %03f, %03f, %03f\n", diffuse0[0], diffuse0[1], diffuse0[2]);

  glBegin(GL_POLYGON);
  for (int i = 0; i < POINTNUM; i++) {
    float normal[] = {curve[i].x, CYLINDER_Y+y, curve[i].z-CYLINDER_Z};

    float color0[3] = {0, 0, 0}, color1[3] = {0, 0, 0}, color2[3] = {0, 0, 0};
    calc_light(curve[i], normal, direction0, ambient0, diffuse0, specular0, color0);
    calc_light(curve[i], normal, direction1, ambient1, diffuse1, specular1, color1);
    calc_light(curve[i], normal, direction2, ambient2, diffuse2, specular2, color2);

    float color[3] = {0, 0, 0};
    vector_add_vector(color, color0, color);
    vector_add_vector(color, color1, color);
    vector_add_vector(color, color2, color);
    vector_multiply_num(color, 0.333f, color);
    glColor3fv(color);
    glVertex3f(curve[i].x, CYLINDER_Y+y, curve[i].z);
  }
  glEnd();
}

// generate the coordinates of a circle
void GenerateCurve() {
  for (int i = 0; i < POINTNUM; i++) {
    curve[i].x = RADIUS * cos(float(i*STEP)*PI/180.0f);
    curve[i].y = 0;
    curve[i].z = CYLINDER_Z + RADIUS * sin(float(i*STEP)*PI/180.0f);
  }
}

// generate the cylinder, it's made by a stack of circles
void GenerateSurface() {
  GenerateCurve();
  glNewList(CYLINDER, GL_COMPILE);
    for (int i = 0; i < CIRCLENUM; i++) {
      DrawCurve(i/4.0f);
    }
  glEndList();
}

// setup 3 light sources
void set_lighting() {
  GLfloat ambient0[] = {0.6, 0.6, 0.6, 1.0};
  GLfloat ambient1[] = {0.6, 0.6, 0.6, 1.0};
  GLfloat ambient2[] = {0.6, 0.6, 0.6, 1.0};
  GLfloat diffuse0[] = {1.0, 0.0, 0.0, 1.0};
  GLfloat diffuse1[] = {0.0, 1.0, 0.0, 1.0};
  GLfloat diffuse2[] = {0.0, 0.0, 1.0, 1.0};
  GLfloat specular[] = {1.0, 1.0, 1.0, 1.0};
  GLfloat ldir0[] = {-20.0, 12.0, 30.0+CYLINDER_Z, 0.0}; // top-left corner
  GLfloat ldir1[] = {20.0, 12.0, 30.0+CYLINDER_Z, 0.0}; // top-right corner
  GLfloat ldir2[] = {0.0, 0.0, 2.0, 0.0}; // directly behind viewer

  glLightfv(GL_LIGHT0, GL_AMBIENT, ambient0);
  glLightfv(GL_LIGHT0, GL_DIFFUSE, diffuse0);
  glLightfv(GL_LIGHT0, GL_SPECULAR, specular);
  glLightfv(GL_LIGHT0, GL_POSITION, ldir0);
  glLightfv(GL_LIGHT1, GL_AMBIENT, ambient1);
  glLightfv(GL_LIGHT1, GL_DIFFUSE, diffuse1);
  glLightfv(GL_LIGHT1, GL_SPECULAR, specular);
  glLightfv(GL_LIGHT1, GL_POSITION, ldir1);
  glLightfv(GL_LIGHT2, GL_AMBIENT, ambient2);
  glLightfv(GL_LIGHT2, GL_DIFFUSE, diffuse2);
  glLightfv(GL_LIGHT2, GL_SPECULAR, specular);
  glLightfv(GL_LIGHT2, GL_POSITION, ldir2);

  // because we will calculate light by ourselves, we don't enable opengl light here
//  glEnable(GL_LIGHT0);
//  glEnable(GL_LIGHT1);
//  glEnable(GL_LIGHT2);
//  glEnable(GL_LIGHTING);
}

void init() {
  glutInitDisplayMode(GLUT_DOUBLE | GLUT_RGB | GLUT_DEPTH);
  glutInitWindowSize(360, 360);
  glutCreateWindow( "Cylinder Calculate Light" );

  glEnable(GL_DEPTH_TEST);
  glPolygonMode(GL_FRONT_AND_BACK, GL_LINE);
  set_lighting();
  GenerateSurface();
  glShadeModel(GL_SMOOTH);
}

GLvoid display() {
  glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);

  glColor3f(1.0f, 0, 0); // not effect now, coz lighting enable
  glCallList(CYLINDER);

  glutSwapBuffers();
}

void reshape( int w, int h ) {
  glViewport( 0, 0, w, h );
  glMatrixMode( GL_PROJECTION );
  glLoadIdentity();
  gluPerspective( 90.0f, (GLfloat)w / (GLfloat)h, 1.0f, 60.0f );
  glMatrixMode( GL_MODELVIEW );
  glLoadIdentity();
  // place eye at (0, 0, 0)
  gluLookAt(0.0, 0.0, 0.0, 0.0, 0.0, -1.0, 0.0, 1.0, 0.0);
}

void keyboard( unsigned char key, int x, int y ) {
    switch ( key ) {
        case 'q':
        case 'Q':
        case 27:  
            exit(0);
            break;
        default : 
            break;
    }
}

int main( int argc, char **argv ) {
    //glutInit( &argc, argv );
    init();

    glutDisplayFunc( display );
    glutReshapeFunc( reshape );
    glutKeyboardFunc( keyboard );
    glutMainLoop();
    return 0;
}
