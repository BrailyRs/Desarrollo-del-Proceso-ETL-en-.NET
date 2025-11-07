# main.py
from fastapi import FastAPI, Depends
from pydantic import BaseModel
from sqlalchemy import create_engine, Column, Integer, String, DateTime, ForeignKey
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

class Fuente(Base):
    __tablename__ = 'Fuentes'
    IdFuente = Column(Integer, primary_key=True, index=True)
    Nombre = Column(String)

class Comentario(Base):
    __tablename__ = 'Comentarios'
    IdComment = Column(String, primary_key=True)
    Fecha = Column(DateTime, primary_key=True)
    IdCliente = Column(Integer, ForeignKey('Clientes.IdCliente'))
    IdProducto = Column(Integer, ForeignKey('Productos.IdProducto'))
    IdFuente = Column(Integer, ForeignKey('Fuentes.IdFuente'))
    Comentario = Column('Comentario', String)

    cliente = relationship("Cliente")
    producto = relationship("Producto")
    fuente = relationship("Fuente")

# --- Modelos de Pydantic para la respuesta de la API ---
# Esto define la estructura del JSON de salida

class ClienteResponse(BaseModel):
    IdCliente: int
    Nombre: str

class ProductoResponse(BaseModel):
    IdProducto: int
    Nombre: str

class FuenteResponse(BaseModel):
    IdFuente: int
    Nombre: str

class ComentarioResponse(BaseModel):
    IdComment: str
    Fecha: datetime
    Comentario: Optional[str]
    cliente: ClienteResponse
    producto: ProductoResponse
    fuente: FuenteResponse

    class Config:
        from_attributes = True

# --- Lógica de la API con FastAPI ---

app = FastAPI(title="Social Media Comments API")

# Dependencia para obtener la sesión de la base de datos
def get_db():
    db = SessionLocal()
    try:
        yield db
    finally:
        db.close()

@app.get("/comentarios", response_model=List[ComentarioResponse], summary="Obtener todos los comentarios de redes sociales")
def read_comentarios(db: Session = Depends(get_db)):
    """
    Endpoint para obtener todos los comentarios de la tabla `Comentarios`,
    incluyendo la información relacionada de Cliente, Producto y Fuente.
    """
    comentarios = db.query(Comentario).options(
        joinedload(Comentario.cliente),
        joinedload(Comentario.producto),
        joinedload(Comentario.fuente)
    ).all()
    return comentarios

# --- Punto de entrada para ejecutar la API (opcional, para desarrollo) ---

if __name__ == "__main__":
    import uvicorn
    print("Para iniciar la API, ejecuta en tu terminal:")
    print(f"cd \"C:\\Users\\PC\\Desktop\\Tareas ITLA\\Electiva 1 - Big Data\\Unidad 4\\SocialMediaApi\"")
    print("uvicorn main:app --reload")
    # uvicorn.run(app, host="127.0.0.1", port=8000)
