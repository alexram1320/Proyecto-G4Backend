using Google.Cloud.Firestore;

namespace Infrastructure.Firebase
{
    public sealed class ContextoFirestore(FirestoreDb baseDatos)
    {
        public FirestoreDb BaseDatos { get; } = baseDatos;
        public CollectionReference Coleccion(string nombre) => BaseDatos.Collection(nombre);
    }
}
