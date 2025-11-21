# main.py
from fastapi import FastAPI, Depends, Query
from pydantic import BaseModel
from sqlalchemy import create_engine, Column, Integer, String, DateTime, ForeignKey, Float
from sqlalchemy.orm import sessionmaker, Session, relationship, declarative_base, joinedload
from typing import List, Optional
import urllib
from datetime import datetime

# --- Configuración de la Base de Datos de Origen (OpinionDB) ---

# Parámetros de conexión
server = 'DESKTOP-ENGONQR'
database = 'OpinionDB'

# Codificar los parámetros para la URL de conexión
params = urllib.parse.quote_plus(f'DRIVER={{ODBC Driver 17 for SQL Server}};SERVER={server};DATABASE={database};Trusted_Connection=yes')

# Crear la URL de conexión de SQLAlchemy
DATABASE_URL = f"mssql+pyodbc:///?odbc_connect={params}"

# Crear el motor de SQLAlchemy
engine = create_engine(DATABASE_URL)

# Crear una fábrica de sesiones
SessionLocal = sessionmaker(autocommit=False, autoflush=False, bind=engine)

# Base para los modelos declarativos de SQLAlchemy
Base = declarative_base()

# --- Modelos de SQLAlchemy para OpinionDB ---

class Cliente(Base):
    __tablename__ = 'Clientes'
    IdCliente = Column(Integer, primary_key=True, index=True)
    Nombre = Column(String)
    Email = Column(String)

class Producto(Base):
    __tablename__ = 'Productos'
    IdProducto = Column(Integer, primary_key=True, index=True)
    Nombre = Column(String)
    IdCategoria = Column(Integer)

class Categoria(Base):
    __tablename__ = 'Categorias'
    IdCategoria = Column(Integer, primary_key=True, index=True)
    Nombre = Column(String)

class Fuente(Base):
    __tablename__ = 'Fuentes'
    IdFuente = Column(Integer, primary_key=True, index=True)
    Nombre = Column(String)

class Clasificacion(Base):
    __tablename__ = 'Clasificaciones'
    IdClasificacion = Column(Integer, primary_key=True, index=True)
    Nombre = Column(String)

class Comentario(Base):
    __tablename__ = 'Comentarios'
    IdComment = Column(String, primary_key=True)
    Fecha = Column(DateTime, primary_key=True)
    IdCliente = Column(Integer, ForeignKey('Clientes.IdCliente'))
    IdProducto = Column(Integer, ForeignKey('Productos.IdProducto'))
    IdFuente = Column(Integer, ForeignKey('Fuentes.IdFuente'))
    Comentario = Column('Comentario', String)
    SentimientoScore = Column(Float)

    cliente = relationship("Cliente")
    producto = relationship("Producto")
    fuente = relationship("Fuente")

class Encuesta(Base):
    __tablename__ = 'Encuestas'
    IdOpinion = Column(Integer, primary_key=True)
    Fecha = Column(DateTime, primary_key=True)
    IdCliente = Column(Integer, ForeignKey('Clientes.IdCliente'))
    IdProducto = Column(Integer, ForeignKey('Productos.IdProducto'))
    IdCarga = Column(Integer)
    Comentario = Column(String)
    IdClasificacion = Column(Integer, ForeignKey('Clasificaciones.IdClasificacion'))
    PuntajeSatisfaccion = Column(Integer)
    SentimientoScore = Column(Float)

    cliente = relationship("Cliente")
    producto = relationship("Producto")
    clasificacion = relationship("Clasificacion")

class WebReview(Base):
    __tablename__ = 'WebReviews'
    IdReview = Column(String, primary_key=True)
    Fecha = Column(DateTime, primary_key=True)
    IdCliente = Column(Integer, ForeignKey('Clientes.IdCliente'))
    IdProducto = Column(Integer, ForeignKey('Productos.IdProducto'))
    IdCarga = Column(Integer)
    Comentario = Column(String)
    Rating = Column(Integer)
    SentimientoScore = Column(Float)

    cliente = relationship("Cliente")
    producto = relationship("Producto")

# --- Modelos de Pydantic para la respuesta de la API ---
# Esto define la estructura del JSON de salida

class ClienteResponse(BaseModel):
    IdCliente: int
    Nombre: str
    Email: Optional[str]

    class Config:
        from_attributes = True

class ProductoResponse(BaseModel):
    IdProducto: int
    Nombre: str

    class Config:
        from_attributes = True

class FuenteResponse(BaseModel):
    IdFuente: int
    Nombre: str

    class Config:
        from_attributes = True

class ClasificacionResponse(BaseModel):
    IdClasificacion: int
    Nombre: str

    class Config:
        from_attributes = True

class ComentarioResponse(BaseModel):
    IdComment: str
    Fecha: datetime
    Comentario: Optional[str]
    SentimientoScore: Optional[float]
    cliente: ClienteResponse
    producto: ProductoResponse
    fuente: FuenteResponse

    class Config:
        from_attributes = True

class EncuestaResponse(BaseModel):
    IdOpinion: int
    Fecha: datetime
    Comentario: Optional[str]
    PuntajeSatisfaccion: Optional[int]
    SentimientoScore: Optional[float]
    cliente: ClienteResponse
    producto: ProductoResponse
    clasificacion: Optional[ClasificacionResponse]

    class Config:
        from_attributes = True

class WebReviewResponse(BaseModel):
    IdReview: str
    Fecha: datetime
    Comentario: Optional[str]
    Rating: Optional[int]
    SentimientoScore: Optional[float]
    cliente: ClienteResponse
    producto: ProductoResponse

    class Config:
        from_attributes = True

# --- Lógica de la API con FastAPI ---

app = FastAPI(
    title="Opinion Analytics API",
    description="API para acceder a datos de opiniones de clientes desde múltiples fuentes",
    version="2.0"
)

# Dependencia para obtener la sesión de la base de datos
def get_db():
    db = SessionLocal()
    try:
        yield db
    finally:
        db.close()

@app.get("/", summary="Información de la API")
def read_root():
    """
    Endpoint raíz que proporciona información sobre la API.
    """
    return {
        "message": "Opinion Analytics API",
        "version": "2.0",
        "endpoints": {
            "comentarios": "/comentarios",
            "encuestas": "/encuestas",
            "reviews": "/reviews",
            "clientes": "/clientes",
            "productos": "/productos",
            "fuentes": "/fuentes"
        }
    }

@app.get("/comentarios", response_model=List[ComentarioResponse], summary="Obtener comentarios de redes sociales")
def read_comentarios(
    skip: int = Query(0, ge=0, description="Número de registros a saltar"),
    limit: int = Query(100, ge=1, le=1000, description="Número máximo de registros a retornar"),
    db: Session = Depends(get_db)
):
    """
    Endpoint para obtener comentarios de redes sociales de la tabla `Comentarios`,
    incluyendo la información relacionada de Cliente, Producto y Fuente.
    """
    comentarios = db.query(Comentario).options(
        joinedload(Comentario.cliente),
        joinedload(Comentario.producto),
        joinedload(Comentario.fuente)
    ).order_by(Comentario.IdComment).offset(skip).limit(limit).all()
    return comentarios

@app.get("/encuestas", response_model=List[EncuestaResponse], summary="Obtener encuestas")
def read_encuestas(
    skip: int = Query(0, ge=0, description="Número de registros a saltar"),
    limit: int = Query(100, ge=1, le=1000, description="Número máximo de registros a retornar"),
    db: Session = Depends(get_db)
):
    """
    Endpoint para obtener encuestas de la tabla `Encuestas`,
    incluyendo la información relacionada de Cliente, Producto y Clasificación.
    """
    encuestas = db.query(Encuesta).options(
        joinedload(Encuesta.cliente),
        joinedload(Encuesta.producto),
        joinedload(Encuesta.clasificacion)
    ).order_by(Encuesta.IdOpinion).offset(skip).limit(limit).all()
    return encuestas

@app.get("/reviews", response_model=List[WebReviewResponse], summary="Obtener reviews web")
def read_reviews(
    skip: int = Query(0, ge=0, description="Número de registros a saltar"),
    limit: int = Query(100, ge=1, le=1000, description="Número máximo de registros a retornar"),
    db: Session = Depends(get_db)
):
    """
    Endpoint para obtener reviews web de la tabla `WebReviews`,
    incluyendo la información relacionada de Cliente y Producto.
    """
    reviews = db.query(WebReview).options(
        joinedload(WebReview.cliente),
        joinedload(WebReview.producto)
    ).order_by(WebReview.IdReview).offset(skip).limit(limit).all()
    return reviews

@app.get("/clientes", response_model=List[ClienteResponse], summary="Obtener todos los clientes")
def read_clientes(
    skip: int = Query(0, ge=0),
    limit: int = Query(100, ge=1, le=1000),
    db: Session = Depends(get_db)
):
    """
    Endpoint para obtener la lista de clientes.
    """
    clientes = db.query(Cliente).order_by(Cliente.IdCliente).offset(skip).limit(limit).all()
    return clientes

@app.get("/productos", response_model=List[ProductoResponse], summary="Obtener todos los productos")
def read_productos(
    skip: int = Query(0, ge=0),
    limit: int = Query(100, ge=1, le=1000),
    db: Session = Depends(get_db)
):
    """
    Endpoint para obtener la lista de productos.
    """
    productos = db.query(Producto).order_by(Producto.IdProducto).offset(skip).limit(limit).all()
    return productos

@app.get("/fuentes", response_model=List[FuenteResponse], summary="Obtener todas las fuentes")
def read_fuentes(db: Session = Depends(get_db)):
    """
    Endpoint para obtener la lista de fuentes de datos.
    """
    fuentes = db.query(Fuente).all()
    return fuentes

# --- Punto de entrada para ejecutar la API (opcional, para desarrollo) ---

if __name__ == "__main__":
    import uvicorn
    print("Para iniciar la API, ejecuta en tu terminal:")
    print(f"cd \"C:\\Users\\PC\\Desktop\\Tareas ITLA\\Electiva 1 - Big Data\\Unidad 5\\SocialMediaApi\"")
    print("uvicorn main:app --reload")
    print("\nO ejecuta directamente:")
    uvicorn.run(app, host="127.0.0.1", port=8000, reload=True)

