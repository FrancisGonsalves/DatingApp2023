export interface Group
{
    name: string;
    connections: Connection[]
}
export interface Connection
{
    userName: string;
    connectionId: string;
}